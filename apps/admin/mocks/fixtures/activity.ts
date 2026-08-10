import type { ActivityEvent } from '@/lib/types'

interface MockActivityEvent extends ActivityEvent {
  actorId: string
}

export const activity: MockActivityEvent[] = [
  { id: 'activity-1', actorId: 'user-editor', actorName: 'Jordan Blake', action: 'published "How to Debug Anything"', createdAt: '2026-08-10T08:00:00Z' },
  { id: 'activity-2', actorId: 'user-owner', actorName: 'Hayk Baroyan', action: "changed Alex Rivera's role to Editor", createdAt: '2026-08-09T15:30:00Z' },
  { id: 'activity-3', actorId: 'user-editor', actorName: 'Jordan Blake', action: 'hid "Interviews"', createdAt: '2026-08-08T11:00:00Z' },
  { id: 'activity-4', actorId: 'user-owner', actorName: 'Hayk Baroyan', action: "approved Sam Okafor's application", createdAt: '2026-08-08T09:00:00Z' },
]
