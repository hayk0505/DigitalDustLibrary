import { createFileRoute } from '@tanstack/react-router'

export const Route = createFileRoute('/_authenticated/')({
  component: () => <div>Dashboard lands in Task 20.</div>,
})
