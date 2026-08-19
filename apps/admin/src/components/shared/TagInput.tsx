import { useState } from 'react'

export type TagOption = { id: string; name: string; isNew?: boolean }

export function filterSuggestions(query: string, suggestions: TagOption[], selected: TagOption[]): TagOption[] {
  const q = query.trim().toLowerCase()
  if (!q) return []
  const selectedIds = new Set(selected.map((t) => t.id))
  return suggestions.filter((t) => !selectedIds.has(t.id) && t.name.toLowerCase().includes(q))
}

export function shouldOfferCreate(query: string, allTags: TagOption[]): boolean {
  const q = query.trim()
  if (!q) return false
  return !allTags.some((t) => t.name.toLowerCase() === q.toLowerCase())
}

export function TagInput({
  value,
  onChange,
  allTags,
}: {
  value: TagOption[]
  onChange: (next: TagOption[]) => void
  allTags: TagOption[]
}) {
  const [query, setQuery] = useState('')
  const matches = filterSuggestions(query, allTags, value)
  const offerCreate = shouldOfferCreate(query, allTags)

  function addTag(tag: TagOption) {
    onChange([...value, tag])
    setQuery('')
  }

  function removeTag(id: string) {
    onChange(value.filter((t) => t.id !== id))
  }

  return (
    <div className="space-y-2">
      {value.length > 0 && (
        <div className="flex flex-wrap gap-2">
          {value.map((tag) => (
            <span
              key={tag.id}
              className="inline-flex items-center gap-1 rounded-full bg-secondary px-2.5 py-0.5 text-xs text-secondary-foreground"
            >
              {tag.name}
              <button
                type="button"
                onClick={() => removeTag(tag.id)}
                aria-label={`Remove ${tag.name}`}
                className="cursor-pointer"
              >
                ×
              </button>
            </span>
          ))}
        </div>
      )}
      <input
        type="text"
        value={query}
        onChange={(e) => setQuery(e.target.value)}
        placeholder="Type to add a tag…"
        className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-xs outline-none"
      />
      {(matches.length > 0 || offerCreate) && (
        <div className="rounded-md border border-border bg-popover text-sm">
          {matches.map((tag) => (
            <button
              key={tag.id}
              type="button"
              onClick={() => addTag(tag)}
              className="block w-full cursor-pointer px-3 py-1.5 text-left hover:bg-accent"
            >
              {tag.name}
            </button>
          ))}
          {offerCreate && (
            <button
              type="button"
              onClick={() => addTag({ id: crypto.randomUUID(), name: query.trim(), isNew: true })}
              className="block w-full cursor-pointer px-3 py-1.5 text-left hover:bg-accent"
            >
              Create "{query.trim()}"
            </button>
          )}
        </div>
      )}
    </div>
  )
}
