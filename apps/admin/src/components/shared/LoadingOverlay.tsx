import { Loader2 } from 'lucide-react'

export function LoadingOverlay({ show }: { show: boolean }) {
  if (!show) return null

  return (
    <div className="fixed inset-0 z-[60] flex w-full items-center justify-center bg-background/70">
      <Loader2 className="size-8 animate-spin text-muted-foreground" />
    </div>
  )
}
