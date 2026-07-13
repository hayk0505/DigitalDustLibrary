import { cn } from '@/lib/utils'

interface FilterChipsProps {
  options: { value: string; label: string }[]
  value: string
  onChange: (value: string) => void
}

export function FilterChips({ options, value, onChange }: FilterChipsProps) {
  return (
    <div className="inline-flex gap-1 rounded-full border border-border bg-card p-1">
      {options.map((option) => (
        <button
          key={option.value}
          type="button"
          onClick={() => onChange(option.value)}
          className={cn(
            'rounded-full px-3 py-1 text-sm transition-colors',
            option.value === value
              ? 'bg-primary text-primary-foreground'
              : 'text-muted-foreground hover:text-foreground',
          )}
        >
          {option.label}
        </button>
      ))}
    </div>
  )
}
