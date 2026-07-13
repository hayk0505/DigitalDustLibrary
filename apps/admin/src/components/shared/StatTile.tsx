import { cn } from '@/lib/utils'

interface Trend {
  direction: 'up' | 'down'
  value: string
}

export function StatTile({ label, value, trend }: { label: string; value: string | number; trend?: Trend }) {
  return (
    <div className="rounded-2xl border border-border bg-card p-5 shadow-elevated">
      <p className="font-mono text-[11px] uppercase tracking-wide text-muted-foreground">{label}</p>
      <p className="font-heading mt-2 text-2xl text-foreground">{value}</p>
      {trend && (
        <p
          data-testid="stat-trend"
          className={cn('mt-1 text-xs', trend.direction === 'up' ? 'text-status-published-fg' : 'text-status-changes-fg')}
        >
          {trend.value}
        </p>
      )}
    </div>
  )
}
