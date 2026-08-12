import { describe, expect, it, vi, afterEach } from 'vitest'
import { render, screen, fireEvent, waitFor, within } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { setAuthState } from '@/lib/auth-store'
import type { ManagedUser } from '@/lib/types'
import { Users } from './Users'

vi.mock('@tanstack/react-router', () => ({
  useNavigate: () => vi.fn(),
}))

const owner = { id: 'owner-1', name: 'Owner', email: 'owner@dd.local', role: 'owner' as const }
const target: ManagedUser = {
  id: 'target-1',
  name: 'Target Author',
  email: 'target@example.com',
  role: 'author',
  isActive: true,
  createdAt: '2026-01-01T00:00:00Z',
}
// A real prod incident: a name stored with a trailing space (from
// unsanitized direct-add/application input) made the confirm-name gate
// impossible to satisfy — nobody can see or type an invisible trailing
// space, so the Delete button stayed disabled forever no matter what was
// typed.
const targetWithWhitespace: ManagedUser = {
  id: 'target-2',
  name: 'Seda ',
  email: 'seda@example.com',
  role: 'author',
  isActive: true,
  createdAt: '2026-01-01T00:00:00Z',
}

function jsonResponse(body: unknown, status = 200) {
  return new Response(JSON.stringify(body), { status, headers: { 'Content-Type': 'application/json' } })
}

describe('Users — delete flow', () => {
  const originalFetch = globalThis.fetch

  afterEach(() => {
    globalThis.fetch = originalFetch
    vi.restoreAllMocks()
    setAuthState({ accessToken: null, user: null })
  })

  it('enables the Delete button once the typed name matches and the impact check has resolved', async () => {
    setAuthState({ accessToken: 'tok', user: owner })

    const fetchSpy = vi.fn<typeof fetch>(async (input) => {
      const url = typeof input === 'string' ? input : (input as Request).url
      if (url.endsWith('/api/users')) return jsonResponse([owner, target])
      if (url.includes('/deletion-impact')) {
        return jsonResponse({ postCount: 0, mediaCount: 0, reviewNoteCount: 0, affectedOtherPostCount: 0 })
      }
      throw new Error(`Unhandled fetch in test: ${url}`)
    })
    globalThis.fetch = fetchSpy

    const queryClient = new QueryClient()
    render(
      <QueryClientProvider client={queryClient}>
        <Users />
      </QueryClientProvider>,
    )

    await screen.findByText('Target Author')

    const row = screen.getByText('Target Author').closest('tr')!
    fireEvent.click(within(row).getByRole('button', { name: 'Delete' }))

    const dialog = await screen.findByRole('dialog')
    fireEvent.change(within(dialog).getByLabelText(/Type "Target Author" to confirm/), {
      target: { value: 'Target Author' },
    })

    const confirmDeleteButton = within(dialog).getByRole('button', { name: 'Delete' })
    await waitFor(() => expect(confirmDeleteButton).not.toBeDisabled())
  })

  it('enables Delete when the typed name matches a stored name that has a trailing space', async () => {
    setAuthState({ accessToken: 'tok', user: owner })

    const fetchSpy = vi.fn<typeof fetch>(async (input) => {
      const url = typeof input === 'string' ? input : (input as Request).url
      if (url.endsWith('/api/users')) return jsonResponse([owner, targetWithWhitespace])
      if (url.includes('/deletion-impact')) {
        return jsonResponse({ postCount: 0, mediaCount: 0, reviewNoteCount: 0, affectedOtherPostCount: 0 })
      }
      throw new Error(`Unhandled fetch in test: ${url}`)
    })
    globalThis.fetch = fetchSpy

    const queryClient = new QueryClient()
    render(
      <QueryClientProvider client={queryClient}>
        <Users />
      </QueryClientProvider>,
    )

    await screen.findByText('Seda')

    const row = screen.getByText('Seda').closest('tr')!
    fireEvent.click(within(row).getByRole('button', { name: 'Delete' }))

    const dialog = await screen.findByRole('dialog')
    // The label must show the trimmed name — asking someone to type an
    // invisible trailing space is unfixable from the UI.
    within(dialog).getByLabelText('Type "Seda" to confirm')
    fireEvent.change(within(dialog).getByLabelText('Type "Seda" to confirm'), {
      target: { value: 'Seda' },
    })

    const confirmDeleteButton = within(dialog).getByRole('button', { name: 'Delete' })
    await waitFor(() => expect(confirmDeleteButton).not.toBeDisabled())
  })

  it('keeps the dialog open until the delete response comes back, then closes it', async () => {
    setAuthState({ accessToken: 'tok', user: owner })

    let resolveDelete!: (value: Response) => void
    const deleteResponsePromise = new Promise<Response>((resolve) => {
      resolveDelete = resolve
    })

    const fetchSpy = vi.fn<typeof fetch>(async (input, init) => {
      const url = typeof input === 'string' ? input : (input as Request).url
      if (url.endsWith('/api/users')) return jsonResponse([owner, target])
      if (url.includes('/deletion-impact')) {
        return jsonResponse({ postCount: 0, mediaCount: 0, reviewNoteCount: 0, affectedOtherPostCount: 0 })
      }
      if (init?.method === 'DELETE') return deleteResponsePromise
      throw new Error(`Unhandled fetch in test: ${url}`)
    })
    globalThis.fetch = fetchSpy

    const queryClient = new QueryClient()
    render(
      <QueryClientProvider client={queryClient}>
        <Users />
      </QueryClientProvider>,
    )

    await screen.findByText('Target Author')
    const row = screen.getByText('Target Author').closest('tr')!
    fireEvent.click(within(row).getByRole('button', { name: 'Delete' }))

    const dialog = await screen.findByRole('dialog')
    fireEvent.change(within(dialog).getByLabelText(/Type "Target Author" to confirm/), {
      target: { value: 'Target Author' },
    })
    const confirmDeleteButton = within(dialog).getByRole('button', { name: 'Delete' })
    await waitFor(() => expect(confirmDeleteButton).not.toBeDisabled())

    fireEvent.click(confirmDeleteButton)

    // The DELETE request hasn't resolved yet — the dialog must still be open.
    await waitFor(() => expect(within(dialog).getByRole('button', { name: 'Deleting…' })).toBeDisabled())
    expect(screen.getByRole('dialog')).toBeInTheDocument()

    resolveDelete(new Response(null, { status: 204 }))

    await waitFor(() => expect(screen.queryByRole('dialog')).not.toBeInTheDocument())
  })
})
