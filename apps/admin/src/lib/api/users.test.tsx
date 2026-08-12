import { afterEach, describe, expect, it, vi } from 'vitest'
import type { ReactNode } from 'react'
import { QueryClientProvider } from '@tanstack/react-query'
import { renderHook, waitFor } from '@testing-library/react'
import { toast } from 'sonner'
import { queryClient } from '@/lib/queryClient'
import { useDeleteUser, useUserDeletionImpact } from './users'

function wrapper({ children }: { children: ReactNode }) {
  return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
}

function jsonResponse(body: unknown, status = 200): Response {
  return new Response(JSON.stringify(body), { status, headers: { 'Content-Type': 'application/json' } })
}

describe('useDeleteUser', () => {
  const originalFetch = globalThis.fetch

  afterEach(() => {
    globalThis.fetch = originalFetch
    vi.restoreAllMocks()
    queryClient.clear()
  })

  // Real prod scenario: DeleteUserDialog keeps its deletion-impact query
  // mounted and enabled until the confirm dialog's `open` state flips false,
  // which happens in a callback that runs AFTER useDeleteUser's own
  // onSuccess. Invalidating ['users'] without `exact: true` prefix-matches
  // ['users', id, 'deletion-impact'] for the user that was just deleted,
  // triggering an immediate refetch that's guaranteed to 404 (the row is
  // already gone) — surfacing as a spurious error toast right on top of the
  // "User deleted" success toast.
  it('does not refetch the deleted user\'s deletion-impact query after a successful delete', async () => {
    const userId = 'user-1'
    let impactCallCount = 0
    const fetchSpy = vi.fn<typeof fetch>(async (input) => {
      const url = typeof input === 'string' ? input : (input as Request).url
      if (url.includes('/deletion-impact')) {
        impactCallCount += 1
        return jsonResponse({ postCount: 0, mediaCount: 0, reviewNoteCount: 0, affectedOtherPostCount: 0 })
      }
      if (url.endsWith(`/users/${userId}`)) return new Response(null, { status: 204 })
      throw new Error(`Unhandled fetch in test: ${url}`)
    })
    globalThis.fetch = fetchSpy
    const toastError = vi.spyOn(toast, 'error').mockImplementation(() => '')

    const { result: impact } = renderHook(() => useUserDeletionImpact(userId, { enabled: true }), { wrapper })
    await waitFor(() => expect(impact.current.isSuccess).toBe(true))
    expect(impactCallCount).toBe(1)

    const { result: del } = renderHook(() => useDeleteUser(), { wrapper })
    del.current.mutate(userId)
    await waitFor(() => expect(del.current.isSuccess).toBe(true))

    // Give a spurious refetch a moment to fire before asserting it didn't.
    await new Promise((resolve) => setTimeout(resolve, 50))

    expect(impactCallCount).toBe(1)
    expect(toastError).not.toHaveBeenCalled()
  })
})
