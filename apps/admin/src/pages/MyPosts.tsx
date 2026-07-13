import { useState } from 'react'
import { Link } from '@tanstack/react-router'
import { usePosts } from '@/lib/api/posts'
import { FilterChips } from '@/components/shared/FilterChips'
import { StatusBadge } from '@/components/shared/StatusBadge'
import { PillarTag } from '@/components/shared/PillarTag'
import { formatRelativeTime } from '@/lib/formatting'
import type { PostStatus } from '@/lib/types'

const FILTERS: { value: PostStatus | 'all'; label: string }[] = [
  { value: 'all', label: 'All' },
  { value: 'draft', label: 'Draft' },
  { value: 'pending_review', label: 'Pending Review' },
  { value: 'changes_requested', label: 'Changes Requested' },
  { value: 'published', label: 'Published' },
]

export function MyPosts() {
  const [filter, setFilter] = useState<PostStatus | 'all'>('all')
  const { data: posts = [], isLoading } = usePosts({ mine: true })
  const filtered = filter === 'all' ? posts : posts.filter((p) => p.status === filter)

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="font-heading text-2xl text-foreground">My Posts</h1>
        <Link to="/posts/new" className="rounded-lg bg-primary px-4 py-2 text-sm text-primary-foreground">
          New post
        </Link>
      </div>

      <FilterChips options={FILTERS} value={filter} onChange={(v) => setFilter(v as PostStatus | 'all')} />

      {isLoading ? (
        <p className="text-muted-foreground">Loading…</p>
      ) : (
        <div className="divide-y divide-border rounded-2xl border border-border bg-card">
          {filtered.map((post) => (
            <Link key={post.id} to="/posts/$postId" params={{ postId: post.id }} className="flex items-center justify-between p-4 hover:bg-accent">
              <div className="min-w-0">
                <p className="truncate text-sm text-foreground">{post.title}</p>
                <div className="mt-1 flex items-center gap-2">
                  <PillarTag pillar={post.pillar} />
                  <span className="text-xs text-muted-foreground">Updated {formatRelativeTime(post.updatedAt)}</span>
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
