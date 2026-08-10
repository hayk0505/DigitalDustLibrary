import { useRef, useState } from 'react'
import { toast } from 'sonner'
import { useMedia, useUploadMedia } from '@/lib/api/media'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import type { MediaAsset } from '@/lib/types'

function readFileAsDataUrl(file: File): Promise<string> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader()
    reader.onload = () => resolve(reader.result as string)
    reader.onerror = () => reject(reader.error)
    reader.readAsDataURL(file)
  })
}

function readImageDimensions(dataUrl: string): Promise<{ width: number; height: number }> {
  return new Promise((resolve, reject) => {
    const image = new Image()
    image.onload = () => resolve({ width: image.naturalWidth, height: image.naturalHeight })
    image.onerror = () => reject(new Error('Could not read image dimensions'))
    image.src = dataUrl
  })
}

export function MediaLibrary({ onSelect }: { onSelect?: (asset: MediaAsset) => void }) {
  const [search, setSearch] = useState('')
  const { data: assets = [], isLoading } = useMedia(search)
  const upload = useUploadMedia()
  const fileInputRef = useRef<HTMLInputElement>(null)

  function handleUploadClick() {
    fileInputRef.current?.click()
  }

  async function handleFileChange(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0]
    e.target.value = '' // allow re-selecting the same file consecutively

    if (!file) return

    try {
      const dataUrl = await readFileAsDataUrl(file)
      const { width, height } = await readImageDimensions(dataUrl)
      upload.mutate({ filename: file.name, dataUrl, tag: 'inline', width, height })
    } catch {
      toast.error('Could not read that file as an image')
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="font-heading text-2xl text-foreground">Media Library</h1>
        <input
          ref={fileInputRef}
          type="file"
          accept="image/*"
          className="hidden"
          onChange={handleFileChange}
        />
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
