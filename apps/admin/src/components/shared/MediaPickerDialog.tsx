import { Dialog, DialogContent, DialogDescription, DialogTitle } from '@/components/ui/dialog'
import { MediaLibrary } from '@/pages/MediaLibrary'
import type { MediaAsset } from '@/lib/types'

interface MediaPickerDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  title: string
  onSelect: (asset: MediaAsset) => void
  onDeleted?: (asset: MediaAsset) => void
}

// Shared by the featured-image picker (PostEditor) and the inline-image
// picker (TipTapEditor). The dialog caps its height at 85vh and lays out as a
// column, so MediaLibrary's asset grid scrolls inside it instead of the whole
// dialog growing past the viewport as the library fills up.
// The width has to be sm:max-w-4xl, not max-w-4xl: DialogContent's base
// sm:max-w-lg would otherwise win from sm up and squeeze the grid to ~500px.
export function MediaPickerDialog({ open, onOpenChange, title, onSelect, onDeleted }: MediaPickerDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="flex max-h-[85vh] flex-col sm:max-w-4xl">
        <DialogTitle>{title}</DialogTitle>
        <DialogDescription className="sr-only">
          Choose an image from the media library or upload a new one.
        </DialogDescription>
        <MediaLibrary onSelect={onSelect} onDeleted={onDeleted} />
      </DialogContent>
    </Dialog>
  )
}
