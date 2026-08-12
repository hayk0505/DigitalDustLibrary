export function CategoryTag({ name, color }: { name: string; color: string }) {
  return (
    <span className="inline-flex items-center gap-1.5 text-sm text-foreground">
      <span data-testid="category-dot" className="size-2 rounded-full" style={{ backgroundColor: color }} />
      {name}
    </span>
  )
}
