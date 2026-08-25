import { useState } from 'react'
import { Link, useNavigate } from '@tanstack/react-router'
import { usePosts, useDeletePost } from '@/lib/api/posts'
import { useMedia } from '@/lib/api/media'
import { useAuth } from '@/hooks/useAuth'
import { Avatar } from '@/components/shared/Avatar'
import { CategoryTag } from '@/components/shared/CategoryTag'
import { StatusBadge } from '@/components/shared/StatusBadge'
import { PostContentPreview } from '@/components/shared/PostContentPreview'
import { LoadingOverlay } from '@/components/shared/LoadingOverlay'
import { Button } from '@/components/ui/button'
import { Dialog, DialogContent, DialogTitle, DialogDescription, DialogFooter } from '@/components/ui/dialog'
import { formatRelativeTime, estimateReadTime } from '@/lib/formatting'

export function PostViewer({ postId }: { postId: string }) {
  const navigate = useNavigate()
  const { user } = useAuth()
  const isReviewer = user?.role === 'editor' || user?.role === 'owner'
  const { data: posts = [] } = usePosts({ mine: !isReviewer })
  const post = posts.find((p) => p.id === postId)
  const { data: mediaAssets = [] } = useMedia()
  const featuredImage = post ? mediaAssets.find((m) => m.id === post.featuredImageId) : undefined
  const deletePost = useDeletePost()
  const [confirmDeleteOpen, setConfirmDeleteOpen] = useState(false)

  if (!post) return <p className="text-muted-foreground">Loading…</p>

  function confirmDelete() {
    deletePost.mutate(postId, { onSuccess: () => navigate({ to: '/posts' }) })
  }

  return (
    <div className="mx-auto max-w-3xl space-y-6">
      <Link to="/posts" className="block text-sm text-muted-foreground hover:text-foreground 
      mb-[10px]">
        ← Back to posts
      </Link>

      <div className="flex items-start justify-between gap-4">
        <div className="space-y-3">
          <div className="flex items-center gap-2">
            <StatusBadge status={post.status} />
            <CategoryTag name={post.categoryName} color={post.categoryColor} />
          </div>
          <h1 className="font-heading text-2xl text-foreground">{post.title}</h1>
          <div className="flex items-center gap-3">
            <Avatar name={post.authorName} size="sm" />
            <div className="text-sm text-muted-foreground">
              {post.authorName} · {formatRelativeTime(post.updatedAt)} · {estimateReadTime(post.bodyHtml)}
            </div>
          </div>
        </div>
        <div className="flex shrink-0 gap-2">
          <Link to="/posts/$postId/edit" params={{ postId }} className="rounded-md bg-primary px-4 py-2 text-sm text-primary-foreground">
            Edit
          </Link>
          <Button type="button" variant="destructive" onClick={() => setConfirmDeleteOpen(true)}>
            Delete
          </Button>
        </div>
      </div>

      <div className="rounded-2xl border border-border bg-card p-6">
        <PostContentPreview
          tags={post.tags}
          excerpt={post.excerpt}
          featuredImageUrl={featuredImage?.url}
          bodyHtml={post.bodyHtml}
        />
      </div>

      <LoadingOverlay show={deletePost.isPending} />

      <Dialog open={confirmDeleteOpen} onOpenChange={setConfirmDeleteOpen}>
        <DialogContent>
          <DialogTitle>Delete post</DialogTitle>
          <DialogDescription>
            Permanently delete "{post.title}"? This cannot be undone.
          </DialogDescription>
          <DialogFooter>
            <Button variant="secondary" onClick={() => setConfirmDeleteOpen(false)}>Cancel</Button>
            <Button variant="destructive" disabled={deletePost.isPending} onClick={confirmDelete}>
              {deletePost.isPending ? 'Deleting…' : 'Delete'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}
