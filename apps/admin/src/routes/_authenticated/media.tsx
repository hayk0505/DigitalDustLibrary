import { createFileRoute } from '@tanstack/react-router'
import { MediaLibrary } from '@/pages/MediaLibrary'

export const Route = createFileRoute('/_authenticated/media')({
  component: MediaLibrary,
})
