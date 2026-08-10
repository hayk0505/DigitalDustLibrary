import { http, HttpResponse } from 'msw'
import { posts } from '../fixtures/posts'
import { decodeMockToken, findUserById } from '../fixtures/users'
import type { Post } from '@/lib/types'

function currentUserId(request: Request): string | null {
  const token = request.headers.get('authorization')?.replace('Bearer ', '')
  return token ? (decodeMockToken(token)?.sub ?? null) : null
}

export const postHandlers = [
  http.get('/api/posts', ({ request }) => {
    const url = new URL(request.url)
    const mine = url.searchParams.get('mine') === 'true'
    const userId = currentUserId(request)
    const result = mine && userId ? posts.filter((p) => p.authorId === userId) : posts
    return HttpResponse.json(result)
  }),

  http.post('/api/posts', async ({ request }) => {
    const userId = currentUserId(request) ?? 'user-author'
    const body = (await request.json()) as Partial<Post>
    const created: Post = {
      id: `post-${posts.length + 1}`,
      title: body.title ?? 'Untitled draft',
      bodyHtml: body.bodyHtml ?? '',
      excerpt: body.excerpt ?? '',
      seoTitle: body.seoTitle ?? '',
      metaDescription: body.metaDescription ?? '',
      featuredImageId: body.featuredImageId ?? null,
      pillar: body.pillar ?? 'tech',
      status: body.status ?? 'draft',
      authorId: userId,
      authorName: findUserById(userId)?.name ?? '',
      updatedAt: new Date().toISOString(),
      publishedAt: null,
      latestReviewNote: null,
    }
    posts.push(created)
    return HttpResponse.json(created, { status: 201 })
  }),

  http.patch('/api/posts/:id', async ({ params, request }) => {
    const post = posts.find((p) => p.id === params.id)
    if (!post) return HttpResponse.json({ message: 'Not found' }, { status: 404 })
    const body = (await request.json()) as Partial<Post>
    Object.assign(post, body, { updatedAt: new Date().toISOString() })
    return HttpResponse.json(post)
  }),
]
