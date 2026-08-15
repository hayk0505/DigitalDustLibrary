import type { ManagedUser } from '@/lib/types'

export const managedUsers: ManagedUser[] = [
  {
    id: 'user-author',
    name: 'Alex Rivera',
    email: 'author@dd.local',
    role: 'author',
    isActive: true,
    createdAt: '2026-06-01T09:00:00Z',
    bio: null,
  },
  {
    id: 'user-editor',
    name: 'Jordan Blake',
    email: 'editor@dd.local',
    role: 'editor',
    isActive: true,
    createdAt: '2026-05-15T09:00:00Z',
    bio: null,
  },
  {
    id: 'user-owner',
    name: 'Hayk Baroyan',
    email: 'owner@dd.local',
    role: 'owner',
    isActive: true,
    createdAt: '2026-01-10T09:00:00Z',
    bio: 'Software engineer exploring technology, software, and internet culture.',
  },
]
