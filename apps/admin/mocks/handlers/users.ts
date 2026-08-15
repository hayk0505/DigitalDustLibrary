import { http, HttpResponse } from 'msw'
import { managedUsers } from '../fixtures/managed-users'
import type { ManagedUser } from '@/lib/types'

export const userHandlers = [
  http.get('/api/users', () => HttpResponse.json(managedUsers)),

  http.patch('/api/users/:id', async ({ params, request }) => {
    const user = managedUsers.find((u) => u.id === params.id)
    if (!user) return HttpResponse.json({ message: 'Not found' }, { status: 404 })
    const body = (await request.json()) as Partial<Pick<ManagedUser, 'role' | 'isActive' | 'bio'>>
    Object.assign(user, body)
    return HttpResponse.json(user)
  }),
]
