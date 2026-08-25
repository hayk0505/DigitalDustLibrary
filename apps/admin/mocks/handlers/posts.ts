import { http, HttpResponse } from 'msw'
import { posts } from '../fixtures/posts'
import { categories } from '../fixtures/categories'
import { decodeMockToken, findUserById } from '../fixtures/users'
import type { Post } from '@/lib/types'

function currentUserId(request: Request): string | null {
  const token = request.headers.get('authorization')?.replace('Bearer ', '')
  return token ? (decodeMockToken(token)?.sub ?? null) : null
}

export function applyPostPatch(id: string, body: Partial<Post>): Post | null {
  const post = posts.find((p) => p.id === id)
  if (!post) return null
  const categoryPatch = body.categoryId
    ? {
        categoryName: categories.find((c) => c.id === body.categoryId)?.name ?? '',
        categoryColor: categories.find((c) => c.id === body.categoryId)?.color ?? '',
        categoryFolderColor: categories.find((c) => c.id === body.categoryId)?.folderColor ?? null,
      }
    : {}
  Object.assign(post, body, categoryPatch, { updatedAt: new Date().toISOString() })
  return post
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
      categoryId: body.categoryId ?? 'cat-tech',
      categoryName: body.categoryId ? (categories.find((c) => c.id === body.categoryId)?.name ?? '') : 'Tech',
      categoryColor: body.categoryId ? (categories.find((c) => c.id === body.categoryId)?.color ?? '') : '#C9553D',
      categoryFolderColor: body.categoryId ? (categories.find((c) => c.id === body.categoryId)?.folderColor ?? null) : null,
      format: body.format ?? 'shelf',
      status: body.status ?? 'draft',
      authorId: userId,
      authorName: findUserById(userId)?.name ?? '',
      updatedAt: new Date().toISOString(),
      publishedAt: null,
      tags: body.tags ?? [],
      latestReviewNote: null,
    }
    posts.push(created)
    return HttpResponse.json(created, { status: 201 })
  }),

  http.patch('/api/posts/:id', async ({ params, request }) => {
    const body = (await request.json()) as Partial<Post>
    const updated = applyPostPatch(params.id as string, body)
    if (!updated) return HttpResponse.json({ message: 'Not found' }, { status: 404 })
    return HttpResponse.json(updated)
  }),
]
