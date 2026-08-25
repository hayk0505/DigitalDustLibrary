import { createFileRoute } from '@tanstack/react-router'
import { PostEditor } from '@/pages/PostEditor'

export const Route = createFileRoute('/_authenticated/posts/$postId_/edit')({
  component: PostIdEditRoute,
})

function PostIdEditRoute() {
  const { postId } = Route.useParams()
  return <PostEditor postId={postId} />
}
