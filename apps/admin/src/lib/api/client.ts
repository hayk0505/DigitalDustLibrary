import { getAuthState, setAuthState } from '@/lib/auth-store'

export class ApiError extends Error {
  status: number
  constructor(status: number, message: string) {
    super(message)
    this.status = status
  }
}

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '/api'

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

export async function apiFetch<T>(path: string, init: RequestInit = {}): Promise<T> {
  let response = await rawFetch(path, init)

  if (response.status === 401 && getAuthState().accessToken) {
    const refreshRes = await rawFetch('/auth/refresh', { method: 'POST' })
    if (refreshRes.ok) {
      const { accessToken } = (await refreshRes.json()) as { accessToken: string }
      setAuthState({ accessToken, user: getAuthState().user })
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
