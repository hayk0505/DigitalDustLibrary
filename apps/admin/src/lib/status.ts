import type { Pillar, PostStatus } from './types'

const STATUS_MAP: Record<PostStatus, { bg: string; fg: string; label: string }> = {
  draft: { bg: 'bg-status-draft-bg', fg: 'text-status-draft-fg', label: 'Draft' },
  pending_review: { bg: 'bg-status-pending-bg', fg: 'text-status-pending-fg', label: 'Pending Review' },
  changes_requested: { bg: 'bg-status-changes-bg', fg: 'text-status-changes-fg', label: 'Changes Requested' },
  published: { bg: 'bg-status-published-bg', fg: 'text-status-published-fg', label: 'Published' },
}

export function getStatusColors(status: PostStatus) {
  return STATUS_MAP[status]
}

const PILLAR_MAP: Record<Pillar, { bg: string; label: string }> = {
  tech: { bg: 'bg-pillar-tech', label: 'Tech' },
  social_psych: { bg: 'bg-pillar-social', label: 'Social & Psychological' },
  software_dev: { bg: 'bg-pillar-dev', label: 'Software Development' },
}

export function getPillarColor(pillar: Pillar) {
  return PILLAR_MAP[pillar]
}
