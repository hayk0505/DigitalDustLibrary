import { http, HttpResponse } from 'msw'
import { mediaAssets } from '../fixtures/media'
import type { MediaAsset } from '@/lib/types'

export const mediaHandlers = [
  http.get('/api/media', ({ request }) => {
    const url = new URL(request.url)
    const search = url.searchParams.get('search')?.toLowerCase()
    const result = search ? mediaAssets.filter((m) => m.filename.toLowerCase().includes(search)) : mediaAssets
    return HttpResponse.json(result)
  }),

  http.post('/api/media', async ({ request }) => {
    const body = (await request.json()) as { filename: string; dataUrl: string; tag: MediaAsset['tag']; width: number; height: number }
    const created: MediaAsset = {
      id: `media-${mediaAssets.length + 1}`,
      filename: body.filename,
      tag: body.tag,
      width: body.width,
      height: body.height,
      url: body.dataUrl,
    }
    mediaAssets.push(created)
    return HttpResponse.json(created, { status: 201 })
  }),
]
