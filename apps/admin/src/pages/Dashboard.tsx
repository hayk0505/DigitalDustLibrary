import { Link } from '@tanstack/react-router'
import { useAuth } from '@/hooks/useAuth'
import { usePosts } from '@/lib/api/posts'
import { useApplications } from '@/lib/api/applications'
import { useActivity } from '@/lib/api/activity'
import { useUsers } from '@/lib/api/users'
import { StatTile } from '@/components/shared/StatTile'
import { Card } from '@/components/shared/Card'
import { StatusBadge } from '@/components/shared/StatusBadge'
import { CategoryTag } from '@/components/shared/CategoryTag'
import { Avatar } from '@/components/shared/Avatar'
import { formatRelativeTime } from '@/lib/formatting'
import type { Post } from '@/lib/types'

function countByStatus(posts: Post[], status: Post['status']): number {
  return posts.filter((p) => p.status === status).length
}

function isThisMonth(dateStr: string): boolean {
  const date = new Date(dateStr)
  const now = new Date()
  return date.getFullYear() === now.getFullYear() && date.getMonth() === now.getMonth()
}

function isReviewDecision(action: string): boolean {
  return action.startsWith('published "') || action.startsWith('requested changes on "')
}

export function Dashboard() {
  const { user } = useAuth()
  return user?.role === 'author' ? <AuthorDashboard /> : <ReviewerDashboard />
}

function AuthorDashboard() {
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
                    <CategoryTag name={post.categoryName} color={post.categoryColor} />
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

function ReviewerDashboard() {
  const { user } = useAuth()
  const isOwner = user?.role === 'owner'
  const { data: posts = [] } = usePosts()
  const { data: applications = [] } = useApplications()
  const { data: activity = [] } = useActivity({ mine: true })
  const { data: users = [] } = useUsers({ enabled: isOwner })

  const pendingReview = posts.filter((p) => p.status === 'pending_review')
  const changesRequestedCount = countByStatus(posts, 'changes_requested')
  const publishedThisMonth = posts.filter((p) => p.status === 'published' && p.publishedAt && isThisMonth(p.publishedAt)).length
  const pendingApplications = applications.filter((a) => a.status === 'pending')
  const reviewedTotal = activity.filter((a) => isReviewDecision(a.action)).length
  const activeAuthors = users.filter((u) => u.role === 'author' && u.isActive).length

  return (
    <div className="space-y-8">
      <div>
        <h1 className="font-heading text-2xl text-foreground">Good morning, {user?.name.split(' ')[0]}</h1>
        <p className="text-muted-foreground">Here's what's waiting on your review.</p>
      </div>

      <div className="grid grid-cols-4 gap-4">
        <StatTile label="Pending review" value={pendingReview.length} />
        {isOwner ? (
          <StatTile label="Applications" value={pendingApplications.length} />
        ) : (
          <StatTile label="Changes requested" value={changesRequestedCount} />
        )}
        <StatTile label="Published/mo" value={publishedThisMonth} />
        {isOwner ? (
          <StatTile label="Active authors" value={activeAuthors} />
        ) : (
          <StatTile label="Reviewed total" value={reviewedTotal} />
        )}
      </div>

      <div className="grid grid-cols-3 gap-6">
        <Card className="col-span-2">
          <h2 className="font-heading mb-4 text-lg text-foreground">Review queue</h2>
          <div className="space-y-3">
            {pendingReview.slice(0, 4).map((post) => (
              <Link key={post.id} to="/review/$postId" params={{ postId: post.id }} className="flex items-center justify-between rounded-lg border border-border p-3 hover:bg-accent">
                <div className="min-w-0">
                  <p className="truncate text-sm text-foreground">{post.title}</p>
                  <div className="mt-1 flex items-center gap-2">
                    <CategoryTag name={post.categoryName} color={post.categoryColor} />
                    <span className="text-xs text-muted-foreground">
                      {post.authorName} · {formatRelativeTime(post.updatedAt)}
                    </span>
                  </div>
                </div>
                <StatusBadge status={post.status} />
              </Link>
            ))}
            {pendingReview.length === 0 && <p className="text-sm text-muted-foreground">Nothing pending review.</p>}
          </div>
        </Card>

        <div className="space-y-6">
          <Card>
            <h2 className="font-heading mb-4 text-lg text-foreground">Applications</h2>
            <div className="space-y-3">
              {pendingApplications.slice(0, 3).map((application) => (
                <Link key={application.id} to="/applications" className="block rounded-lg border border-border p-3 hover:bg-accent">
                  <p className="text-sm text-foreground">{application.name}</p>
                  <p className="mt-1 truncate text-xs text-muted-foreground">{application.pitch}</p>
                </Link>
              ))}
              {pendingApplications.length === 0 && <p className="text-sm text-muted-foreground">No pending applications.</p>}
            </div>
          </Card>

          <Card>
            <h2 className="font-heading mb-4 text-lg text-foreground">Your recent activity</h2>
            <div className="space-y-3">
              {activity.slice(0, 5).map((event) => (
                <div key={event.id} className="flex items-center gap-2">
                  <Avatar name={event.actorName} size="sm" />
                  <div className="min-w-0 flex-1">
                    <p className="truncate text-xs text-foreground">{event.action}</p>
                    <p className="text-xs text-muted-foreground">{formatRelativeTime(event.createdAt)}</p>
                  </div>
                </div>
              ))}
              {activity.length === 0 && <p className="text-sm text-muted-foreground">No activity yet.</p>}
            </div>
          </Card>
        </div>
      </div>
    </div>
  )
}
