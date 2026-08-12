import { describe, expect, it, vi, afterEach } from 'vitest'
import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { setAuthState } from '@/lib/auth-store'
import { PostEditor } from './PostEditor'

vi.mock('@tanstack/react-router', () => ({
  useNavigate: () => vi.fn(),
}))

// TipTapEditor mounts a real ProseMirror instance, which is unrelated to the
// category-default bug under test here and adds real risk of its own
// (contentEditable/selection APIs jsdom doesn't fully implement). Stubbing it
// keeps this test scoped to PostEditor's own form-default wiring.
vi.mock('@/components/shared/TipTapEditor', () => ({
  TipTapEditor: () => null,
}))

function jsonResponse(body: unknown) {
  return new Response(JSON.stringify(body), { status: 200, headers: { 'Content-Type': 'application/json' } })
}

describe('PostEditor — new-post category default', () => {
  const originalFetch = globalThis.fetch

  afterEach(() => {
    globalThis.fetch = originalFetch
    vi.restoreAllMocks()
    setAuthState({ accessToken: null, user: null })
  })

  it('backfills categoryId to the first loaded category once the async categories fetch resolves, for a brand-new post', async () => {
    // Regression test for: react-hook-form's `defaultValues` is a snapshot
    // read once at mount. `useCategories()` resolves asynchronously and is
    // `[]` on that first render, so without a reactive fix, a new post's
    // `categoryId` locks in as '' forever — the zod schema then silently
    // blocks every "Save draft"/"Publish" click, with no visible error since
    // this file never renders `formState.errors`.
    setAuthState({
      accessToken: 'tok',
      user: { id: 'author-1', name: 'Alex', email: 'alex@dd.local', role: 'author' },
    })

    // A holder object, not a directly-reassigned `let`: TypeScript's control-flow
    // narrowing for a `let` only tracks assignments in the *same* function scope,
    // so a `let` mutated only inside this nested `fetchSpy` closure gets narrowed
    // (incorrectly, for our purposes) down to its initial `null` at every read
    // site below — an object property doesn't fall into that trap.
    const created: { body: Record<string, unknown> | null } = { body: null }
    const fetchSpy = vi.fn<typeof fetch>(async (input, init) => {
      const url = typeof input === 'string' ? input : (input as Request).url
      if (url.includes('/api/posts') && init?.method === 'POST') {
        created.body = JSON.parse(init.body as string)
        return jsonResponse({
          id: 'post-new',
          ...created.body,
          authorName: 'Alex',
          updatedAt: new Date().toISOString(),
          publishedAt: null,
          latestReviewNote: null,
        })
      }
      if (url.includes('/api/posts')) return jsonResponse([])
      if (url.includes('/api/media')) return jsonResponse([])
      if (url.includes('/api/categories')) {
        return jsonResponse([
          { id: 'cat-tech', name: 'Tech', slug: 'tech', description: '', color: '#C9553D', position: 1, isVisible: true, isDeleted: false, postCount: 0 },
          { id: 'cat-dev', name: 'Software Dev', slug: 'software-dev', description: '', color: '#4A6FBF', position: 2, isVisible: true, isDeleted: false, postCount: 0 },
        ])
      }
      throw new Error(`Unhandled fetch in test: ${url}`)
    })
    globalThis.fetch = fetchSpy

    const queryClient = new QueryClient()
    render(
      <QueryClientProvider client={queryClient}>
        <PostEditor />
      </QueryClientProvider>,
    )

    // Synchronize on the categories query actually resolving into the cache
    // (not just on `fetch` having been *called*, which happens at request
    // start rather than once the response — and the effect it triggers —
    // have landed).
    await waitFor(() => expect(queryClient.getQueryData(['categories'])).toBeTruthy())

    fireEvent.change(screen.getByLabelText('Title'), { target: { value: 'A New Post' } })
    fireEvent.click(screen.getByRole('button', { name: 'Save draft' }))

    await waitFor(() => expect(created.body).not.toBeNull())
    if (!created.body) throw new Error('expected created.body to have been set by the mocked POST /api/posts call')
    expect(created.body.categoryId).toBe('cat-tech')
  })
})
