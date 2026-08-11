import type { AuthorApplication } from '@/lib/types'

export const applications: AuthorApplication[] = [
  {
    id: 'app-1',
    name: 'Sam Okafor',
    email: 'sam.okafor@example.com',
    pitch: 'I want to write about the psychology of long-term remote work and how it reshapes team trust.',
    status: 'pending',
    submittedAt: '2026-08-08T14:00:00Z',
    reviewedAt: null,
    devInviteUrl: null,
  },
  {
    id: 'app-2',
    name: 'Priya Nandan',
    email: 'priya.nandan@example.com',
    pitch: 'A deep dive into event-sourcing patterns for small teams — when they help and when they are overkill.',
    status: 'approved',
    submittedAt: '2026-08-01T09:30:00Z',
    reviewedAt: '2026-08-02T11:00:00Z',
    devInviteUrl: null,
  },
  {
    id: 'app-3',
    name: 'Marco Lindqvist',
    email: 'marco.lindqvist@example.com',
    pitch: 'Thoughts on why most productivity advice fails engineers specifically.',
    status: 'rejected',
    submittedAt: '2026-07-28T16:45:00Z',
    reviewedAt: '2026-07-29T10:15:00Z',
    devInviteUrl: null,
  },
]
