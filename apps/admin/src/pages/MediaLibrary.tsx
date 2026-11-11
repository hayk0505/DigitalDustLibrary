import { useRef, useState } from 'react'
import { toast } from 'sonner'
import { Trash2 } from 'lucide-react'
import { useMedia, useUploadMedia, useDeleteMedia } from '@/lib/api/media'
import { useAuth } from '@/hooks/useAuth'
import { cn } from '@/lib/utils'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { Dialog, DialogContent, DialogTitle, DialogDescription, DialogFooter } from '@/components/ui/dialog'
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

interface MediaLibraryProps {
  /** Picker mode: clicking an asset selects it instead of just showing it. */
  onSelect?: (asset: MediaAsset) => void
  /** Runs after a successful delete, so a parent holding the asset (a post's featured image, an <img> in the editor) can drop it. */
  onDeleted?: (asset: MediaAsset) => void
}

export function MediaLibrary({ onSelect, onDeleted }: MediaLibraryProps) {
  const isPicker = onSelect !== undefined
  const { user } = useAuth()
  const canDelete = user?.role === 'editor' || user?.role === 'owner'
  const [search, setSearch] = useState('')
  const { data: assets = [], isLoading } = useMedia(search)
  const upload = useUploadMedia()
  const remove = useDeleteMedia()
  const fileInputRef = useRef<HTMLInputElement>(null)
  const [pendingDelete, setPendingDelete] = useState<MediaAsset | null>(null)

  function confirmDelete() {
    if (!pendingDelete) return
    const asset = pendingDelete
    remove.mutate(asset.id, {
      onSuccess: () => onDeleted?.(asset),
      // A 409 (the image is used by a published post) is shown by the
      // global mutation error toast, so the dialog closes either way.
      onSettled: () => setPendingDelete(null),
    })
  }

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

  const searchInput = (
    <Input placeholder="Search assets…" value={search} onChange={(e) => setSearch(e.target.value)} className="max-w-sm" />
  )

  const uploadControls = (
    <>
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
    </>
  )

  return (
    <div className={cn('flex flex-col', isPicker ? 'min-h-0 flex-1 gap-4' : 'gap-6')}>
      {isPicker ? (
        // The dialog already has a title, so search and upload share one row.
        <div className="flex items-center justify-between gap-4">
          {searchInput}
          {uploadControls}
        </div>
      ) : (
        <>
          <div className="flex items-center justify-between">
            <h1 className="font-heading text-2xl text-foreground">Media Library</h1>
            {uploadControls}
          </div>
          {searchInput}
        </>
      )}

      {/* In picker mode only the grid scrolls; search and upload stay pinned
          above it. The px-1/-mx-1 pair keeps card focus rings from being
          clipped by the overflow container. */}
      <div className={cn(isPicker && '-mx-1 min-h-0 flex-1 overflow-y-auto px-1 pb-1')}>
        {isLoading ? (
          <p className="text-muted-foreground">Loading…</p>
        ) : (
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
            {assets.map((asset) => {
              const content = (
                <>
                  <img src={asset.url} alt={asset.filename} className="aspect-video w-full rounded-lg object-cover" />
                  <p className="mt-2 truncate text-sm text-foreground">{asset.filename}</p>
                  <p className="text-xs text-muted-foreground">{asset.tag} · {asset.width}×{asset.height}</p>
                </>
              )
              return (
                <div key={asset.id} className="relative rounded-2xl border border-border bg-card">
                  {/* Select and delete are sibling buttons, not nested: a
                      button inside a button is invalid HTML, and the delete
                      click would also select the asset. */}
                  {onSelect ? (
                    <button
                      type="button"
                      onClick={() => onSelect(asset)}
                      className="w-full rounded-2xl p-3 text-left hover:bg-accent focus-visible:ring-2 focus-visible:ring-ring focus-visible:outline-none"
                    >
                      {content}
                    </button>
                  ) : (
                    <div className="p-3">{content}</div>
                  )}
                  {canDelete && (
                    <Button
                      type="button"
                      variant="destructive"
                      size="icon-sm"
                      className="absolute top-2 right-2"
                      disabled={remove.isPending}
                      onClick={() => setPendingDelete(asset)}
                      aria-label={`Delete ${asset.filename}`}
                      title="Delete"
                    >
                      <Trash2 />
                    </Button>
                  )}
                </div>
              )
            })}
          </div>
        )}
      </div>

      <Dialog open={pendingDelete !== null} onOpenChange={(open) => !open && setPendingDelete(null)}>
        <DialogContent>
          <DialogTitle>Delete asset</DialogTitle>
          <DialogDescription>
            Delete "{pendingDelete?.filename}"? This cannot be undone. Images used by a published post can't be
            deleted until they're removed from that post.
          </DialogDescription>
          <DialogFooter>
            <Button variant="secondary" onClick={() => setPendingDelete(null)}>Cancel</Button>
            <Button variant="destructive" disabled={remove.isPending} onClick={confirmDelete}>
              {remove.isPending ? 'Deleting…' : 'Delete'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}
