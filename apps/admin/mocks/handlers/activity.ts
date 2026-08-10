import { http, HttpResponse } from 'msw'
import { activity } from '../fixtures/activity'
import { decodeMockToken } from '../fixtures/users'

export const activityHandlers = [
  http.get('/api/activity', ({ request }) => {
    const url = new URL(request.url)
    const mine = url.searchParams.get('mine') === 'true'
    if (!mine) return HttpResponse.json(activity)

    const token = request.headers.get('authorization')?.replace('Bearer ', '')
    const decoded = token ? decodeMockToken(token) : null
    const filtered = decoded ? activity.filter((a) => a.actorId === decoded.sub) : []
    return HttpResponse.json(filtered)
  }),
]
