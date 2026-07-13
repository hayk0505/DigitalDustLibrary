import { getPillarColor } from '@/lib/status'
import type { Pillar } from '@/lib/types'
import { cn } from '@/lib/utils'

export function PillarTag({ pillar }: { pillar: Pillar }) {
  const { bg, label } = getPillarColor(pillar)
  return (
    <span className="inline-flex items-center gap-1.5 text-sm text-foreground">
      <span data-testid="pillar-dot" className={cn('size-2 rounded-full', bg)} />
      {label}
    </span>
  )
}
