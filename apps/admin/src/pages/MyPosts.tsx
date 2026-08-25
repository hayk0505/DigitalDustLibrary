import { useMemo, useState } from 'react'
import { Link } from '@tanstack/react-router'
import { useAuth } from '@/hooks/useAuth'
import { usePosts } from '@/lib/api/posts'
import { FilterChips } from '@/components/shared/FilterChips'
import { StatusBadge } from '@/components/shared/StatusBadge'
import { CategoryTag } from '@/components/shared/CategoryTag'
import { formatRelativeTime } from '@/lib/formatting'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import type { PostStatus } from '@/lib/types'

const FILTERS: { value: PostStatus | 'all'; label: string }[] = [
  { value: 'all', label: 'All' },
  { value: 'draft', label: 'Draft' },
  { value: 'pending_review', label: 'Pending Review' },
  { value: 'changes_requested', label: 'Changes Requested' },
  { value: 'published', label: 'Published' },
]

export function MyPosts() {
  const { user } = useAuth()
  const isReviewer = user?.role === 'editor' || user?.role === 'owner'
  const [filter, setFilter] = useState<PostStatus | 'all'>('all')
  const [authorFilter, setAuthorFilter] = useState<string>('all')
  const { data: posts = [], isLoading } = usePosts({ mine: !isReviewer })

  const authors = useMemo(() => {
    const byId = new Map<string, string>()
    for (const post of posts) byId.set(post.authorId, post.authorName)
    return Array.from(byId, ([id, name]) => ({ id, name })).sort((a, b) => a.name.localeCompare(b.name))
  }, [posts])

  const filtered = posts
    .filter((p) => filter === 'all' || p.status === filter)
    .filter((p) => authorFilter === 'all' || p.authorId === authorFilter)

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="font-heading text-2xl text-foreground">{isReviewer ? 'All Posts' : 'My Posts'}</h1>
        <Link to="/posts/new" className="rounded-lg bg-primary px-4 py-2 text-sm text-primary-foreground">
          New post
        </Link>
      </div>

      <div className="flex flex-wrap items-center gap-4">
        <FilterChips options={FILTERS} value={filter} onChange={(v) => setFilter(v as PostStatus | 'all')} />

        {isReviewer && (
          <Select value={authorFilter} onValueChange={setAuthorFilter}>
            <SelectTrigger className="w-48">
              <SelectValue />
            </SelectTrigger>
            <SelectContent position="popper">
              <SelectItem value="all">All authors</SelectItem>
              {authors.map((author) => (
                <SelectItem key={author.id} value={author.id}>
                  {author.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        )}
      </div>

      {isLoading ? (
        <p className="text-muted-foreground">Loading…</p>
      ) : (
        <div className="divide-y divide-border rounded-2xl border border-border bg-card">
          {filtered.map((post) => (
            <Link
              key={post.id}
              to="/posts/$postId"
              params={{ postId: post.id }}
              className="flex items-center justify-between p-4 hover:bg-accent first:rounded-t-2xl last:rounded-b-2xl"
            >
              <div className="min-w-0">
                <p className="truncate text-sm text-foreground">{post.title}</p>
                <div className="mt-1 flex items-center gap-2">
                  <CategoryTag name={post.categoryName} color={post.categoryColor} />
                  <span className="text-xs text-muted-foreground">
                    {isReviewer
                      ? `${post.authorName} · ${formatRelativeTime(post.updatedAt)}`
                      : `Updated ${formatRelativeTime(post.updatedAt)}`}
                  </span>
                </div>
              </div>
              <StatusBadge status={post.status} />
            </Link>
          ))}
          {filtered.length === 0 && <p className="p-6 text-center text-muted-foreground">No posts in this filter.</p>}
        </div>
      )}
    </div>
  )
}
