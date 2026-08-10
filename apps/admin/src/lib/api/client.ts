import { getAuthState, setAuthState } from '@/lib/auth-store'

export class ApiError extends Error {
  status: number
  constructor(status: number, message: string) {
    super(message)
    this.status = status
  }
}

export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '/api'

async function rawFetch(path: string, init: RequestInit): Promise<Response> {
  const token = getAuthState().accessToken
  return fetch(`${API_BASE_URL}${path}`, {
    ...init,
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...init.headers,
    },
  })
}

// Concurrent 401s (e.g. AdminLayout's nav-badge queries firing alongside a
// page's own query, all sharing one now-expired access token) must share a
// single refresh attempt. The API rotates the refresh token on every use
// with no grace window, so an independent second refresh call arriving
// after the first already rotated the cookie gets its session killed (401,
// plus the server deleting the now-stale cookie) even though the first
// refresh just succeeded.
let refreshPromise: Promise<string | null> | null = null

function refreshAccessToken(): Promise<string | null> {
  if (!refreshPromise) {
    refreshPromise = (async () => {
      try {
        const refreshRes = await rawFetch('/auth/refresh', { method: 'POST' })
        if (!refreshRes.ok) return null
        const { accessToken } = (await refreshRes.json()) as { accessToken: string }
        setAuthState({ accessToken, user: getAuthState().user })
        return accessToken
      } finally {
        refreshPromise = null
      }
    })()
  }
  return refreshPromise
}

export async function apiFetch<T>(path: string, init: RequestInit = {}): Promise<T> {
  let response = await rawFetch(path, init)

  if (response.status === 401 && getAuthState().accessToken) {
    const accessToken = await refreshAccessToken()
    if (accessToken) {
      response = await rawFetch(path, init)
    }
  }

  if (!response.ok) {
    const body = await response.json().catch(() => ({ message: response.statusText }))
    throw new ApiError(response.status, body.message ?? 'Request failed')
  }

  if (response.status === 204) return undefined as T
  return (await response.json()) as T
}
