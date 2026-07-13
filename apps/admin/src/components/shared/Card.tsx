import type { ReactNode } from 'react'
import { cn } from '@/lib/utils'

export function Card({ children, className }: { children: ReactNode; className?: string }) {
  return (
    <div className={cn('rounded-2xl border border-border bg-card p-5 shadow-elevated', className)}>
      {children}
    </div>
  )
}
