import { createFileRoute } from '@tanstack/react-router'
import { MyPosts } from '@/pages/MyPosts'

export const Route = createFileRoute('/_authenticated/posts/')({
  component: MyPosts,
})
