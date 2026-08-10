import { http, HttpResponse } from 'msw'
import { categories } from '../fixtures/categories'
import type { Category } from '@/lib/types'

export const categoryHandlers = [
  http.get('/api/categories', () => HttpResponse.json(categories)),

  http.post('/api/categories', async ({ request }) => {
    const body = (await request.json()) as { name: string; slug: string; isPillar?: boolean }
    const created: Category = {
      id: `cat-${categories.length + 1}`,
      name: body.name,
      slug: body.slug,
      isPillar: body.isPillar ?? false,
      isVisible: true,
      isDeleted: false,
      postCount: 0,
    }
    categories.push(created)
    return HttpResponse.json(created, { status: 201 })
  }),

  http.patch('/api/categories/:id', async ({ params, request }) => {
    const category = categories.find((c) => c.id === params.id)
    if (!category) return HttpResponse.json({ message: 'Not found' }, { status: 404 })
    const body = (await request.json()) as Partial<Category>
    Object.assign(category, body)
    return HttpResponse.json(category)
  }),

  http.delete('/api/categories/:id', ({ params }) => {
    const index = categories.findIndex((c) => c.id === params.id)
    if (index === -1) return HttpResponse.json({ message: 'Not found' }, { status: 404 })
    const category = categories[index]!
    if (category.postCount > 0) {
      return HttpResponse.json(
        { message: `Cannot delete '${category.name}' — ${category.postCount} post(s) still reference it.` },
        { status: 409 },
      )
    }
    categories.splice(index, 1)
    return new HttpResponse(null, { status: 204 })
  }),
]
