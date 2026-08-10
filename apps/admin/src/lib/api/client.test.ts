import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { apiFetch } from './client'
import { setAuthState } from '@/lib/auth-store'

function jsonResponse(body: unknown, status = 200): Response {
  return new Response(JSON.stringify(body), { status, headers: { 'Content-Type': 'application/json' } })
}

describe('apiFetch', () => {
  const originalFetch = globalThis.fetch

  beforeEach(() => {
    setAuthState({ accessToken: 'stale-token', user: null })
  })

  afterEach(() => {
    globalThis.fetch = originalFetch
    vi.restoreAllMocks()
  })

  it('shares one refresh call across concurrent 401s and retries both callers', async () => {
    let refreshCalls = 0

    globalThis.fetch = vi.fn(async (input: RequestInfo | URL, init?: RequestInit) => {
      const url = String(input)
      const auth = (init?.headers as Record<string, string> | undefined)?.Authorization

      if (url.endsWith('/auth/refresh')) {
        refreshCalls += 1
        return jsonResponse({ accessToken: 'fresh-token' })
      }

      if (url.endsWith('/posts')) {
        // The API rotates the refresh token on every use: a second,
        // independent refresh call arriving after the first already
        // rotated the cookie would 401 and kill a session that had just
        // been validly refreshed. This only ever returns success for the
        // token issued by the (single, deduped) refresh above.
        if (auth === 'Bearer fresh-token') return jsonResponse([{ id: 1 }])
        return jsonResponse({ message: 'Unauthorized' }, 401)
      }

      throw new Error(`Unexpected fetch to ${url}`)
    }) as typeof fetch

    const [a, b] = await Promise.all([apiFetch('/posts'), apiFetch('/posts')])

    expect(a).toEqual([{ id: 1 }])
    expect(b).toEqual([{ id: 1 }])
    expect(refreshCalls).toBe(1)
  })
})
