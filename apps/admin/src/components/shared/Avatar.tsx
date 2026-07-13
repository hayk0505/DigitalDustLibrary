import { getInitials } from '@/lib/formatting'
import { cn } from '@/lib/utils'

const SIZE_CLASSES = {
  sm: 'size-6 text-xs',
  md: 'size-9 text-sm',
  lg: 'size-12 text-base',
} as const

export function Avatar({ name, size = 'md' }: { name: string; size?: keyof typeof SIZE_CLASSES }) {
  return (
    <span className={cn('inline-flex items-center justify-center rounded-full bg-avatar-bg font-medium text-avatar-fg', SIZE_CLASSES[size])}>
      {getInitials(name)}
    </span>
  )
}
