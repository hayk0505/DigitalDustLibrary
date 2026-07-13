import { useState } from 'react'
import { useMedia, useUploadMedia } from '@/lib/api/media'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import type { MediaAsset } from '@/lib/types'

export function MediaLibrary({ onSelect }: { onSelect?: (asset: MediaAsset) => void }) {
  const [search, setSearch] = useState('')
  const { data: assets = [], isLoading } = useMedia(search)
  const upload = useUploadMedia()

  function handleUploadClick() {
    const filename = `upload-${Date.now()}.png`
    upload.mutate({
      filename,
      dataUrl: `data:image/svg+xml;base64,${btoa('<svg xmlns="http://www.w3.org/2000/svg" width="800" height="600"><rect width="100%" height="100%" fill="#A27B5B"/></svg>')}`,
      tag: 'inline',
      width: 800,
      height: 600,
    })
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="font-heading text-2xl text-foreground">Media Library</h1>
        <Button onClick={handleUploadClick} disabled={upload.isPending}>
          {upload.isPending ? 'Uploading…' : 'Upload new'}
        </Button>
      </div>

      <Input placeholder="Search assets…" value={search} onChange={(e) => setSearch(e.target.value)} className="max-w-sm" />

      {isLoading ? (
        <p className="text-muted-foreground">Loading…</p>
      ) : (
        <div className="grid grid-cols-4 gap-4">
          {assets.map((asset) => {
            const content = (
              <>
                <img src={asset.url} alt={asset.filename} className="aspect-video w-full rounded-lg object-cover" />
                <p className="mt-2 truncate text-sm text-foreground">{asset.filename}</p>
                <p className="text-xs text-muted-foreground">{asset.tag} · {asset.width}×{asset.height}</p>
              </>
            )
            return onSelect ? (
              <button key={asset.id} type="button" onClick={() => onSelect(asset)} className="rounded-2xl border border-border bg-card p-3 text-left hover:bg-accent">
                {content}
              </button>
            ) : (
              <div key={asset.id} className="rounded-2xl border border-border bg-card p-3">
                {content}
              </div>
            )
          })}
        </div>
      )}
    </div>
  )
}
