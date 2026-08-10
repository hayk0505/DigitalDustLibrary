import { http, HttpResponse } from 'msw'
import { settings } from '../fixtures/settings'
import type { SiteSettings } from '@/lib/types'

export const settingsHandlers = [
  http.get('/api/settings', () => HttpResponse.json(settings)),

  http.patch('/api/settings', async ({ request }) => {
    const body = (await request.json()) as Omit<SiteSettings, 'id'>
    Object.assign(settings, body)
    return HttpResponse.json(settings)
  }),
]
