export function getInitials(name: string): string {
  const words = name.trim().split(/\s+/).filter(Boolean)
  return words.slice(0, 2).map((w) => w[0]!.toUpperCase()).join('')
}

export function formatRelativeTime(date: string | Date): string {
  const target = typeof date === 'string' ? new Date(date) : date
  const diffMs = Date.now() - target.getTime()
  const diffMinutes = Math.round(diffMs / 60_000)

  if (diffMinutes < 60) return `${Math.max(diffMinutes, 0)}m ago`

  const diffHours = Math.round(diffMinutes / 60)
  if (diffHours < 24) return `${diffHours}h ago`

  const diffDays = Math.round(diffHours / 24)
  return `${diffDays}d ago`
}

export function estimateReadTime(bodyHtml: string): string {
  const text = bodyHtml.replace(/<[^>]*>/g, ' ')
  const words = text.trim().split(/\s+/).filter(Boolean).length
  const minutes = Math.max(1, Math.round(words / 200))
  return `${minutes} min read`
}

export function slugify(name: string): string {
  return name
    .trim()
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '')
}
