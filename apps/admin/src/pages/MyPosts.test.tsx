import { describe, expect, it, vi, afterEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { setAuthState } from '@/lib/auth-store'
import type { Post } from '@/lib/types'
import { MyPosts } from './MyPosts'

vi.mock('@tanstack/react-router', () => ({
  useNavigate: () => vi.fn(),
  Link: ({ children, className }: { children: React.ReactNode; className?: string }) => (
    <a className={className}>{children}</a>
  ),
}))

function jsonResponse(body: unknown) {
  return new Response(JSON.stringify(body), { status: 200, headers: { 'Content-Type': 'application/json' } })
}

function makePost(overrides: Partial<Post>): Post {
  return {
    id: 'post-1',
    title: 'A Post',
    bodyHtml: '',
    excerpt: '',
    seoTitle: '',
    metaDescription: '',
    featuredImageId: null,
    categoryId: 'cat-tech',
    categoryName: 'Tech',
    categoryColor: '#C9553D',
    categoryFolderColor: null,
    status: 'draft',
    authorId: 'author-1',
    authorName: 'Some Author',
    updatedAt: '2026-01-01T00:00:00Z',
    publishedAt: null,
    latestReviewNote: null,
    ...overrides,
  }
}

describe('MyPosts', () => {
  const originalFetch = globalThis.fetch

  afterEach(() => {
    globalThis.fetch = originalFetch
    vi.restoreAllMocks()
    setAuthState({ accessToken: null, user: null })
  })

  it('fetches only the author\'s own posts and hides the author column, for an Author', async () => {
    setAuthState({
      accessToken: 'tok',
      user: { id: 'author-1', name: 'Alex', email: 'alex@dd.local', role: 'author' },
    })

    let requestedUrl = ''
    const fetchSpy = vi.fn<typeof fetch>(async (input) => {
      requestedUrl = typeof input === 'string' ? input : (input as Request).url
      return jsonResponse([makePost({ title: 'My Draft', authorName: 'Alex' })])
    })
    globalThis.fetch = fetchSpy

    const queryClient = new QueryClient()
    render(
      <QueryClientProvider client={queryClient}>
        <MyPosts />
      </QueryClientProvider>,
    )

    expect(await screen.findByText('My Posts')).toBeInTheDocument()
    await screen.findByText('My Draft')

    expect(requestedUrl).toContain('mine=true')
    expect(screen.queryByText('Alex')).not.toBeInTheDocument()
  })

  it('fetches every post and shows who wrote each one, for an Owner', async () => {
    setAuthState({
      accessToken: 'tok',
      user: { id: 'owner-1', name: 'Owner', email: 'owner@dd.local', role: 'owner' },
    })

    let requestedUrl = ''
    const fetchSpy = vi.fn<typeof fetch>(async (input) => {
      requestedUrl = typeof input === 'string' ? input : (input as Request).url
      return jsonResponse([makePost({ title: "Someone Else's Post", authorName: 'Jordan Blake' })])
    })
    globalThis.fetch = fetchSpy

    const queryClient = new QueryClient()
    render(
      <QueryClientProvider client={queryClient}>
        <MyPosts />
      </QueryClientProvider>,
    )

    expect(await screen.findByText('All Posts')).toBeInTheDocument()
    await screen.findByText("Someone Else's Post")

    await waitFor(() => expect(requestedUrl).not.toContain('mine=true'))
    expect(screen.getByText(/Jordan Blake/)).toBeInTheDocument()
  })
})
