import type { MediaAsset } from '@/lib/types'

function placeholder(color: string, w: number, h: number): string {
  const svg = `<svg xmlns="http://www.w3.org/2000/svg" width="${w}" height="${h}"><rect width="100%" height="100%" fill="${color}"/></svg>`
  return `data:image/svg+xml;base64,${btoa(svg)}`
}

export const mediaAssets: MediaAsset[] = [
  { id: 'media-1', filename: 'terminal-glow.png', tag: 'featured', width: 1600, height: 900, url: placeholder('#7E97A8', 1600, 900) },
  { id: 'media-2', filename: 'author-headshot.png', tag: 'avatar', width: 400, height: 400, url: placeholder('#A27B5B', 400, 400) },
  { id: 'media-3', filename: 'og-default.png', tag: 'og_image', width: 1200, height: 630, url: placeholder('#8AA98A', 1200, 630) },
  { id: 'media-4', filename: 'inline-diagram.png', tag: 'inline', width: 1000, height: 600, url: placeholder('#7E97A8', 1000, 600) },
]
