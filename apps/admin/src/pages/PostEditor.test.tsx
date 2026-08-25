import { describe, expect, it, vi, afterEach } from 'vitest'
import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { setAuthState } from '@/lib/auth-store'
import { PostEditor } from './PostEditor'

vi.mock('@tanstack/react-router', () => ({
  useNavigate: () => vi.fn(),
}))

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
    setAuthState({
      accessToken: 'tok',
      user: { id: 'author-1', name: 'Alex', email: 'alex@dd.local', role: 'author' },
    })

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
    setAuthState({
      accessToken: 'tok',
      user: { id: 'author-1', name: 'Alex', email: 'alex@dd.local', role: 'author' },
    })

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

    await waitFor(() => expect(screen.getByRole('button', { name: 'Save draft' })).toBeDisabled())
    expect(screen.getByRole('button', { name: 'Submit for review' })).toBeDisabled()

    fireEvent.click(screen.getByRole('button', { name: 'Save draft' }))
    expect(postCreateCallCount).toBe(0)

    tagCreateResolver.resolve?.()

    await waitFor(() => expect(created.body).not.toBeNull())
    if (!created.body) throw new Error('expected created.body to have been set by the mocked POST /api/posts call')
    expect(created.body.tagIds).toEqual(['tag-new'])
    expect(postCreateCallCount).toBe(1)
  })
})

describe('PostEditor — format', () => {
  const originalFetch = globalThis.fetch

  afterEach(() => {
    globalThis.fetch = originalFetch
    vi.restoreAllMocks()
    setAuthState({ accessToken: null, user: null })
  })

  it('defaults a brand-new post to format "shelf" when the Format field is left untouched', async () => {
    setAuthState({
      accessToken: 'tok',
      user: { id: 'author-1', name: 'Alex', email: 'alex@dd.local', role: 'author' },
    })

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

    fireEvent.change(screen.getByLabelText('Title'), { target: { value: 'A New Post' } })
    fireEvent.click(screen.getByRole('button', { name: 'Save draft' }))

    await waitFor(() => expect(created.body).not.toBeNull())
    if (!created.body) throw new Error('expected created.body to have been set by the mocked POST /api/posts call')
    expect(created.body.format).toBe('shelf')
  })

  it('preserves an existing Mini post\'s format through an edit when the Format field is left untouched', async () => {
    setAuthState({
      accessToken: 'tok',
      user: { id: 'author-1', name: 'Alex', email: 'alex@dd.local', role: 'author' },
    })

    const existingPost = {
      id: 'post-1', title: 'Existing Mini', bodyHtml: '', excerpt: '', seoTitle: '', metaDescription: '',
      featuredImageId: null, categoryId: 'cat-tech', categoryName: 'Tech', categoryColor: '#C9553D',
      categoryFolderColor: null, tags: [], format: 'mini', status: 'draft', authorId: 'author-1',
      authorName: 'Alex', updatedAt: new Date().toISOString(), publishedAt: null, latestReviewNote: null,
    }

    const updated: { body: Record<string, unknown> | null } = { body: null }
    const fetchSpy = vi.fn<typeof fetch>(async (input, init) => {
      const url = typeof input === 'string' ? input : (input as Request).url
      if (url.includes('/api/posts/post-1') && init?.method === 'PATCH') {
        updated.body = JSON.parse(init.body as string)
        return jsonResponse({ ...existingPost, ...updated.body, updatedAt: new Date().toISOString() })
      }
      if (url.includes('/api/posts')) return jsonResponse([existingPost])
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
        <PostEditor postId="post-1" />
      </QueryClientProvider>,
    )

    await waitFor(() => expect(screen.getByDisplayValue('Existing Mini')).toBeInTheDocument())

    fireEvent.click(screen.getByRole('button', { name: 'Save draft' }))

    await waitFor(() => expect(updated.body).not.toBeNull())
    if (!updated.body) throw new Error('expected updated.body to have been set by the mocked PATCH call')
    expect(updated.body.format).toBe('mini')
  })
})
