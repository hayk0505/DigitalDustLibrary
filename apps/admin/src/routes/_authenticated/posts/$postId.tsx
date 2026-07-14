import { createFileRoute } from '@tanstack/react-router'
import { PostEditor } from '@/pages/PostEditor'

export const Route = createFileRoute('/_authenticated/posts/$postId')({
  component: PostIdRoute,
})

function PostIdRoute() {
  const { postId } = Route.useParams()
  return <PostEditor postId={postId} />
}
