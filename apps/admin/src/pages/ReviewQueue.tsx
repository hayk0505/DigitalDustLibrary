import { Link } from '@tanstack/react-router'
import { usePosts } from '@/lib/api/posts'
import { Avatar } from '@/components/shared/Avatar'
import { CategoryTag } from '@/components/shared/CategoryTag'
import { formatRelativeTime } from '@/lib/formatting'

export function ReviewQueue() {
  const { data: posts = [], isLoading } = usePosts()
  const pending = posts.filter((p) => p.status === 'pending_review')

  return (
    <div className="space-y-6">
      <h1 className="font-heading text-2xl text-foreground">Review Queue</h1>

      {isLoading ? (
        <p className="text-muted-foreground">Loading…</p>
      ) : (
        <div className="divide-y divide-border rounded-2xl border border-border bg-card">
          {pending.map((post) => (
            <Link
              key={post.id}
              to="/review/$postId"
              params={{ postId: post.id }}
              className="flex items-center gap-4 p-4 hover:bg-accent"
            >
              <Avatar name={post.authorName} size="sm" />
              <div className="min-w-0 flex-1">
                <p className="truncate text-sm text-foreground">{post.title}</p>
                <div className="mt-1 flex items-center gap-2">
                  <CategoryTag name={post.categoryName} color={post.categoryColor} />
                  <span className="text-xs text-muted-foreground">
                    {post.authorName} · Submitted {formatRelativeTime(post.updatedAt)}
                  </span>
                </div>
                <p className="mt-1 truncate text-sm text-muted-foreground">{post.excerpt}</p>
              </div>
            </Link>
          ))}
          {pending.length === 0 && (
            <p className="p-6 text-center text-muted-foreground">Nothing pending review.</p>
          )}
        </div>
      )}
    </div>
  )
}
