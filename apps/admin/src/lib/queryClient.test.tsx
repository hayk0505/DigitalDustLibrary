import { afterEach, describe, expect, it, vi } from 'vitest'
import type { ReactNode } from 'react'
import { QueryClientProvider, useMutation, useQuery } from '@tanstack/react-query'
import { renderHook, waitFor } from '@testing-library/react'
import { toast } from 'sonner'
import { queryClient, resolveErrorMessage } from './queryClient'
import { apiFetch, ApiError } from './api/client'

function jsonResponse(body: unknown, status = 200): Response {
  return new Response(JSON.stringify(body), { status, headers: { 'Content-Type': 'application/json' } })
}

function wrapper({ children }: { children: ReactNode }) {
  return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
}

describe('resolveErrorMessage', () => {
  it('returns the ApiError message', () => {
    expect(resolveErrorMessage(new ApiError(413, 'Image exceeds the 8 MB upload limit'))).toBe(
      'Image exceeds the 8 MB upload limit',
    )
  })

  it('falls back to a generic message for a non-ApiError error', () => {
    expect(resolveErrorMessage(new TypeError('Failed to fetch'))).toBe('Something went wrong. Please try again.')
  })
})

describe('global error toasts', () => {
  const originalFetch = globalThis.fetch

  afterEach(() => {
    globalThis.fetch = originalFetch
    vi.restoreAllMocks()
    queryClient.clear()
  })

  it('shows a toast when a query fails', async () => {
    globalThis.fetch = vi.fn(async () => jsonResponse({ message: 'Not found' }, 404)) as typeof fetch
    const toastError = vi.spyOn(toast, 'error').mockImplementation(() => '')

    renderHook(
      () => useQuery({ queryKey: ['toast-test-query'], queryFn: () => apiFetch('/missing'), retry: false }),
      { wrapper },
    )

    await waitFor(() => expect(toastError).toHaveBeenCalledWith('Not found', { id: 'Not found' }))
  })

  it('shows a toast when a mutation fails', async () => {
    globalThis.fetch = vi.fn(async () =>
      jsonResponse({ message: 'Image exceeds the 8 MB upload limit' }, 413),
    ) as typeof fetch
    const toastError = vi.spyOn(toast, 'error').mockImplementation(() => '')

    const { result } = renderHook(
      () => useMutation({ mutationFn: () => apiFetch('/media', { method: 'POST' }) }),
      { wrapper },
    )

    result.current.mutate()

    await waitFor(() => expect(toastError).toHaveBeenCalledWith('Image exceeds the 8 MB upload limit', { id: 'Image exceeds the 8 MB upload limit' }))
  })

  it('does not toast when a mutation opts out via meta.skipErrorToast', async () => {
    globalThis.fetch = vi.fn(async () => jsonResponse({ message: 'Invalid email or password' }, 401)) as typeof fetch
    const toastError = vi.spyOn(toast, 'error').mockImplementation(() => '')

    const { result } = renderHook(
      () =>
        useMutation({
          mutationFn: () => apiFetch('/auth/login', { method: 'POST' }),
          meta: { skipErrorToast: true },
        }),
      { wrapper },
    )

    result.current.mutate()

    await waitFor(() => expect(result.current.isError).toBe(true))
    expect(toastError).not.toHaveBeenCalled()
  })
})
