import { createFileRoute } from '@tanstack/react-router'
import { PostEditor } from '@/pages/PostEditor'

export const Route = createFileRoute('/_authenticated/posts/new')({
  component: () => <PostEditor />,
})
