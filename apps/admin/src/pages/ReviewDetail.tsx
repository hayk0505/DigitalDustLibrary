import { useState } from 'react'
import { useNavigate } from '@tanstack/react-router'
import { usePosts, useApprovePost, useRequestChanges } from '@/lib/api/posts'
import { Avatar } from '@/components/shared/Avatar'
import { PillarTag } from '@/components/shared/PillarTag'
import { Card } from '@/components/shared/Card'
import { Button } from '@/components/ui/button'
import { Textarea } from '@/components/ui/textarea'
import { formatRelativeTime, estimateReadTime } from '@/lib/formatting'

export function ReviewDetail({ postId }: { postId: string }) {
  const navigate = useNavigate()
  const { data: posts = [] } = usePosts()
  const post = posts.find((p) => p.id === postId)
  const approve = useApprovePost()
  const requestChanges = useRequestChanges()
  const [comment, setComment] = useState('')

  if (!post) return <p className="text-muted-foreground">Loading…</p>

  const isSubmitting = approve.isPending || requestChanges.isPending

  return (
    <div className="mx-auto grid max-w-5xl grid-cols-3 gap-6">
      <div className="col-span-2 space-y-4">
        <h1 className="font-heading text-2xl text-foreground">{post.title}</h1>
        <div className="flex items-center gap-3">
          <Avatar name={post.authorName} size="sm" />
          <div className="text-sm text-muted-foreground">
            {post.authorName} · {formatRelativeTime(post.updatedAt)} · {estimateReadTime(post.bodyHtml)}
          </div>
          <PillarTag pillar={post.pillar} />
        </div>
        <div className="prose max-w-none" dangerouslySetInnerHTML={{ __html: post.bodyHtml }} />
      </div>

      <Card className="h-fit space-y-4">
        <h2 className="font-heading text-lg text-foreground">Editorial decision</h2>
        <Button
          className="w-full"
          disabled={isSubmitting}
          onClick={() => approve.mutate(post.id, { onSuccess: () => navigate({ to: '/review' }) })}
        >
          Approve & Publish
        </Button>
        <div className="space-y-2">
          <Textarea
            placeholder="What needs to change before this can go live?"
            value={comment}
            onChange={(e) => setComment(e.target.value)}
          />
          <Button
            variant="secondary"
            className="w-full"
            disabled={isSubmitting || comment.trim().length === 0}
            onClick={() =>
              requestChanges.mutate(
                { id: post.id, comment },
                { onSuccess: () => navigate({ to: '/review' }) },
              )
            }
          >
            Request changes
          </Button>
        </div>
      </Card>
    </div>
  )
}
