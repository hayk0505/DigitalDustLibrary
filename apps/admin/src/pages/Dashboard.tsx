import { Link } from '@tanstack/react-router'
import { useAuth } from '@/hooks/useAuth'
import { usePosts } from '@/lib/api/posts'
import { StatTile } from '@/components/shared/StatTile'
import { Card } from '@/components/shared/Card'
import { StatusBadge } from '@/components/shared/StatusBadge'
import { PillarTag } from '@/components/shared/PillarTag'
import { formatRelativeTime } from '@/lib/formatting'
import type { Post } from '@/lib/types'

function countByStatus(posts: Post[], status: Post['status']): number {
  return posts.filter((p) => p.status === status).length
}

export function Dashboard() {
  const { user } = useAuth()
  const { data: posts = [] } = usePosts({ mine: true })
  const changesRequestedPost = posts.find((p) => p.status === 'changes_requested' && p.latestReviewNote)

  return (
    <div className="space-y-8">
      <div>
        <h1 className="font-heading text-2xl text-foreground">Good morning, {user?.name.split(' ')[0]}</h1>
        <p className="text-muted-foreground">Here's what's waiting on you today.</p>
      </div>

      <div className="grid grid-cols-4 gap-4">
        <StatTile label="Drafts" value={countByStatus(posts, 'draft')} />
        <StatTile label="In review" value={countByStatus(posts, 'pending_review')} />
        <StatTile label="Changes requested" value={countByStatus(posts, 'changes_requested')} />
        <StatTile label="Published" value={countByStatus(posts, 'published')} />
      </div>

      <div className="grid grid-cols-3 gap-6">
        <Card className="col-span-2">
          <h2 className="font-heading mb-4 text-lg text-foreground">My posts</h2>
          <div className="space-y-3">
            {posts.slice(0, 4).map((post) => (
              <Link key={post.id} to="/posts/$postId" params={{ postId: post.id }} className="flex items-center justify-between rounded-lg border border-border p-3 hover:bg-accent">
                <div className="min-w-0">
                  <p className="truncate text-sm text-foreground">{post.title}</p>
                  <div className="mt-1 flex items-center gap-2">
                    <PillarTag pillar={post.pillar} />
                    <span className="text-xs text-muted-foreground">{formatRelativeTime(post.updatedAt)}</span>
                  </div>
                </div>
                <StatusBadge status={post.status} />
              </Link>
            ))}
          </div>
        </Card>

        {changesRequestedPost?.latestReviewNote && (
          <Card>
            <h2 className="font-heading mb-2 text-lg text-foreground">Changes requested</h2>
            <p className="text-sm text-muted-foreground">on "{changesRequestedPost.title}"</p>
            <p className="mt-3 text-sm text-foreground">{changesRequestedPost.latestReviewNote.comment}</p>
            <p className="mt-2 text-xs text-muted-foreground">
              — {changesRequestedPost.latestReviewNote.reviewerName}, {formatRelativeTime(changesRequestedPost.latestReviewNote.createdAt)}
            </p>
          </Card>
        )}
      </div>
    </div>
  )
}
