import type { ApplicationStatus, PostStatus } from './types'

const STATUS_MAP: Record<PostStatus, { bg: string; fg: string; label: string }> = {
  draft: { bg: 'bg-status-draft-bg', fg: 'text-status-draft-fg', label: 'Draft' },
  pending_review: { bg: 'bg-status-pending-bg', fg: 'text-status-pending-fg', label: 'Pending Review' },
  changes_requested: { bg: 'bg-status-changes-bg', fg: 'text-status-changes-fg', label: 'Changes Requested' },
  published: { bg: 'bg-status-published-bg', fg: 'text-status-published-fg', label: 'Published' },
}

export function getStatusColors(status: PostStatus) {
  return STATUS_MAP[status]
}

export function getVisibilityColors(isVisible: boolean) {
  return isVisible
    ? { bg: 'bg-status-published-bg', fg: 'text-status-published-fg', label: 'Visible' }
    : { bg: 'bg-status-draft-bg', fg: 'text-status-draft-fg', label: 'Hidden' }
}

export function getCategoryStateColors(isDeleted: boolean) {
  return isDeleted
    ? { bg: 'bg-status-changes-bg', fg: 'text-status-changes-fg', label: 'Deleted' }
    : { bg: 'bg-status-published-bg', fg: 'text-status-published-fg', label: 'Active' }
}

export function getUserStatusColors(isActive: boolean) {
  return isActive
    ? { bg: 'bg-status-published-bg', fg: 'text-status-published-fg', label: 'Active' }
    : { bg: 'bg-status-changes-bg', fg: 'text-status-changes-fg', label: 'Deactivated' }
}

const APPLICATION_STATUS_MAP: Record<ApplicationStatus, { bg: string; fg: string; label: string }> = {
  pending: { bg: 'bg-status-pending-bg', fg: 'text-status-pending-fg', label: 'Pending' },
  approved: { bg: 'bg-status-published-bg', fg: 'text-status-published-fg', label: 'Approved' },
  rejected: { bg: 'bg-status-changes-bg', fg: 'text-status-changes-fg', label: 'Rejected' },
}

export function getApplicationStatusColors(status: ApplicationStatus) {
  return APPLICATION_STATUS_MAP[status]
}
