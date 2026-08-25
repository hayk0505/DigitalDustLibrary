import { createFileRoute } from '@tanstack/react-router'
import { PostViewer } from '@/pages/PostViewer'

export const Route = createFileRoute('/_authenticated/posts/$postId')({
  component: PostIdRoute,
})

function PostIdRoute() {
  const { postId } = Route.useParams()
  return <PostViewer postId={postId} />
}
