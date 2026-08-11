import { describe, expect, it, vi, afterEach } from 'vitest'
import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { ApiError } from '@/lib/api/client'
import { setAuthState } from '@/lib/auth-store'
import { acceptInviteErrorMessage, setPasswordSchema, SetPassword } from './SetPassword'

vi.mock('@tanstack/react-router', () => ({
  useNavigate: () => vi.fn(),
}))

describe('acceptInviteErrorMessage', () => {
  it('returns the server message for an ApiError', () => {
    expect(
      acceptInviteErrorMessage(new ApiError(401, 'This invite link is invalid or has expired.')),
    ).toBe('This invite link is invalid or has expired.')
  })

  it('falls back to a generic message for a non-ApiError error', () => {
    expect(acceptInviteErrorMessage(new TypeError('Failed to fetch'))).toBe(
      'Something went wrong. Please try again.',
    )
  })
})

describe('setPasswordSchema', () => {
  it('accepts matching passwords of at least 8 characters', () => {
    const result = setPasswordSchema.safeParse({
      password: 'longenough',
      confirmPassword: 'longenough',
    })
    expect(result.success).toBe(true)
  })

  it('rejects passwords under 8 characters', () => {
    const result = setPasswordSchema.safeParse({
      password: 'short',
      confirmPassword: 'short',
    })
    expect(result.success).toBe(false)
  })

  it('rejects mismatched passwords, flagging confirmPassword', () => {
    const result = setPasswordSchema.safeParse({
      password: 'longenough',
      confirmPassword: 'different',
    })
    expect(result.success).toBe(false)
    if (!result.success) {
      expect(result.error.issues[0].path).toEqual(['confirmPassword'])
    }
  })
})

describe('SetPassword', () => {
  const originalFetch = globalThis.fetch

  afterEach(() => {
    globalThis.fetch = originalFetch
    vi.restoreAllMocks()
    setAuthState({ accessToken: null, user: null })
  })

  it('forwards the token prop into the accept-invite request body', async () => {
    const fetchSpy = vi.fn<typeof fetch>(async () =>
      new Response(
        JSON.stringify({ accessToken: 'tok', user: { id: '1', name: 'A', email: 'a@b.com', role: 'author' } }),
        { status: 200, headers: { 'Content-Type': 'application/json' } },
      ),
    )
    globalThis.fetch = fetchSpy

    const queryClient = new QueryClient()
    render(
      <QueryClientProvider client={queryClient}>
        <SetPassword token="invite-token-abc" />
      </QueryClientProvider>,
    )

    fireEvent.change(screen.getByLabelText('Password'), { target: { value: 'longenough' } })
    fireEvent.change(screen.getByLabelText('Confirm password'), { target: { value: 'longenough' } })
    fireEvent.click(screen.getByRole('button', { name: /set password/i }))

    await waitFor(() => expect(fetchSpy).toHaveBeenCalled())
    const [, init] = fetchSpy.mock.calls[0]
    expect(JSON.parse((init as RequestInit).body as string)).toEqual({
      token: 'invite-token-abc',
      password: 'longenough',
    })
  })
})
