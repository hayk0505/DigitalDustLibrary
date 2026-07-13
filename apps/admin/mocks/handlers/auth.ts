import { http, HttpResponse } from 'msw'
import { decodeMockToken, encodeMockToken, findAccountByEmail, findUserById } from '../fixtures/users'
import type { AuthResponse } from '@/lib/types'

export const authHandlers = [
  http.post('/api/auth/login', async ({ request }) => {
    const body = (await request.json()) as { email: string; password: string }
    const account = findAccountByEmail(body.email)
    if (!account || account.password !== body.password) {
      return HttpResponse.json({ message: 'Invalid email or password' }, { status: 401 })
    }
    const response: AuthResponse = {
      accessToken: encodeMockToken(account.user),
      user: account.user,
    }
    return HttpResponse.json(response)
  }),

  http.post('/api/auth/refresh', async ({ request }) => {
    const auth = request.headers.get('authorization')
    const token = auth?.replace('Bearer ', '')
    const decoded = token ? decodeMockToken(token) : null
    const user = decoded ? findUserById(decoded.sub) : undefined
    if (!user) {
      return HttpResponse.json({ message: 'Session expired' }, { status: 401 })
    }
    return HttpResponse.json({ accessToken: encodeMockToken(user) })
  }),
]
