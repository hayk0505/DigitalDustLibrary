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

describe('PostEditor — tags', () => {
  const originalFetch = globalThis.fetch

  afterEach(() => {
    globalThis.fetch = originalFetch
    vi.restoreAllMocks()
    setAuthState({ accessToken: null, user: null })
  })

  it('resolves a newly-typed tag via POST /api/tags before submitting the post', async () => {
    setAuthState({
      accessToken: 'tok',
      user: { id: 'author-1', name: 'Alex', email: 'alex@dd.local', role: 'author' },
    })

    const created: { body: Record<string, unknown> | null } = { body: null }
    const fetchSpy = vi.fn<typeof fetch>(async (input, init) => {
      const url = typeof input === 'string' ? input : (input as Request).url
      if (url.includes('/api/tags') && init?.method === 'POST') {
        return jsonResponse({ id: 'tag-new', name: 'Robotics', slug: 'robotics', postCount: 0 })
      }
      if (url.includes('/api/tags')) return jsonResponse([])
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

    await waitFor(() => expect(queryClient.getQueryData(['categories'])).toBeTruthy())

    fireEvent.change(screen.getByLabelText('Title'), { target: { value: 'A Tagged Post' } })
    fireEvent.change(screen.getByPlaceholderText('Type to add a tag…'), { target: { value: 'Robotics' } })
    fireEvent.click(screen.getByRole('button', { name: 'Create "Robotics"' }))
    fireEvent.click(screen.getByRole('button', { name: 'Save draft' }))

    await waitFor(() => expect(created.body).not.toBeNull())
    if (!created.body) throw new Error('expected created.body to have been set by the mocked POST /api/posts call')
    expect(created.body.tagIds).toEqual(['tag-new'])
  })

  it('keeps Save draft/Submit for review disabled for the whole tag-creation round-trip, not just during the post save', async () => {
    // Regression test for: `save()` awaits `resolveTagIds()`, which can fire a
    // real `POST /api/tags` for any freshly-typed tag, before `createPost`/
    // `updatePost` ever gets called. The buttons' `disabled` state used to
    // derive only from `createPost.isPending || updatePost.isPending`, neither
    // of which flips true until *after* that tag round-trip resolves — leaving
    // a window where a second click fires a second, independent `save()` call.
    // `POST /api/posts` has no idempotency guard, so two clicks in that window
    // used to create two separate draft posts instead of one.
    setAuthState({
      accessToken: 'tok',
      user: { id: 'author-1', name: 'Alex', email: 'alex@dd.local', role: 'author' },
    })

    // Held open deliberately (not resolved until the assertions below run)
    // so the test can observe button state *during* the in-flight tag POST,
    // not just before/after it.
    //
    // A holder object, not a directly-reassigned `let`: TypeScript's control-flow
    // narrowing for a `let` only tracks assignments in the *same* function scope,
    // so a `let` mutated only inside this nested `Promise` executor closure gets
    // narrowed (incorrectly, for our purposes) down to its initial `null` at
    // every read site below — an object property doesn't fall into that trap.
    const tagCreateResolver: { resolve: (() => void) | null } = { resolve: null }
    const tagCreatePromise = new Promise<Response>((resolve) => {
      tagCreateResolver.resolve = () => resolve(jsonResponse({ id: 'tag-new', name: 'Robotics', slug: 'robotics', postCount: 0 }))
    })

    const created: { body: Record<string, unknown> | null } = { body: null }
    let postCreateCallCount = 0
    const fetchSpy = vi.fn<typeof fetch>(async (input, init) => {
      const url = typeof input === 'string' ? input : (input as Request).url
      if (url.includes('/api/tags') && init?.method === 'POST') {
        return tagCreatePromise
      }
      if (url.includes('/api/tags')) return jsonResponse([])
      if (url.includes('/api/posts') && init?.method === 'POST') {
        postCreateCallCount += 1
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

    await waitFor(() => expect(queryClient.getQueryData(['categories'])).toBeTruthy())

    fireEvent.change(screen.getByLabelText('Title'), { target: { value: 'A Tagged Post' } })
    fireEvent.change(screen.getByPlaceholderText('Type to add a tag…'), { target: { value: 'Robotics' } })
    fireEvent.click(screen.getByRole('button', { name: 'Create "Robotics"' }))
    fireEvent.click(screen.getByRole('button', { name: 'Save draft' }))

    // The tag-creation POST is still pending here — `resolveTagIds()` hasn't
    // resolved, so `createPost.mutate()` hasn't been called yet, and neither
    // `createPost.isPending` nor `updatePost.isPending` is true. Both buttons
    // must already be disabled by this point.
    await waitFor(() => expect(screen.getByRole('button', { name: 'Save draft' })).toBeDisabled())
    expect(screen.getByRole('button', { name: 'Submit for review' })).toBeDisabled()

    // A second click while disabled must not fire a second `save()` call —
    // browsers (and jsdom) don't dispatch click on a disabled button.
    fireEvent.click(screen.getByRole('button', { name: 'Save draft' }))
    expect(postCreateCallCount).toBe(0)

    tagCreateResolver.resolve?.()

    await waitFor(() => expect(created.body).not.toBeNull())
    if (!created.body) throw new Error('expected created.body to have been set by the mocked POST /api/posts call')
    expect(created.body.tagIds).toEqual(['tag-new'])
    expect(postCreateCallCount).toBe(1)
  })
})
