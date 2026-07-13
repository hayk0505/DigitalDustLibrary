import { getStatusColors } from '@/lib/status'
import type { PostStatus } from '@/lib/types'
import { cn } from '@/lib/utils'

export function StatusBadge({ status }: { status: PostStatus }) {
  const { bg, fg, label } = getStatusColors(status)
  return (
    <span className={cn('inline-flex items-center rounded-full px-2.5 py-0.5 font-mono text-[11px] uppercase tracking-wide', bg, fg)}>
      {label}
    </span>
  )
}
