import { describe, expect, it } from 'vitest'
import { getApplicationStatusColors, getCategoryStateColors, getStatusColors, getUserStatusColors, getVisibilityColors } from './status'

describe('getStatusColors', () => {
  it('maps draft to its token classes and label', () => {
    expect(getStatusColors('draft')).toEqual({
      bg: 'bg-status-draft-bg',
      fg: 'text-status-draft-fg',
      label: 'Draft',
    })
  })

  it('maps changes_requested to its token classes and label', () => {
    expect(getStatusColors('changes_requested')).toEqual({
      bg: 'bg-status-changes-bg',
      fg: 'text-status-changes-fg',
      label: 'Changes Requested',
    })
  })
})

describe('getVisibilityColors', () => {
  it('maps visible to the published tokens', () => {
    expect(getVisibilityColors(true)).toEqual({
      bg: 'bg-status-published-bg',
      fg: 'text-status-published-fg',
      label: 'Visible',
    })
  })

  it('maps hidden to the draft tokens', () => {
    expect(getVisibilityColors(false)).toEqual({
      bg: 'bg-status-draft-bg',
      fg: 'text-status-draft-fg',
      label: 'Hidden',
    })
  })
})

describe('getCategoryStateColors', () => {
  it('maps active to the published tokens', () => {
    expect(getCategoryStateColors(false)).toEqual({
      bg: 'bg-status-published-bg',
      fg: 'text-status-published-fg',
      label: 'Active',
    })
  })

  it('maps deleted to the changes-requested tokens', () => {
    expect(getCategoryStateColors(true)).toEqual({
      bg: 'bg-status-changes-bg',
      fg: 'text-status-changes-fg',
      label: 'Deleted',
    })
  })
})

describe('getUserStatusColors', () => {
  it('maps active to the published tokens', () => {
    expect(getUserStatusColors(true)).toEqual({
      bg: 'bg-status-published-bg',
      fg: 'text-status-published-fg',
      label: 'Active',
    })
  })

  it('maps inactive to the changes-requested tokens', () => {
    expect(getUserStatusColors(false)).toEqual({
      bg: 'bg-status-changes-bg',
      fg: 'text-status-changes-fg',
      label: 'Deactivated',
    })
  })
})

describe('getApplicationStatusColors', () => {
  it('maps pending to the pending tokens', () => {
    expect(getApplicationStatusColors('pending')).toEqual({
      bg: 'bg-status-pending-bg',
      fg: 'text-status-pending-fg',
      label: 'Pending',
    })
  })

  it('maps approved to the published tokens', () => {
    expect(getApplicationStatusColors('approved')).toEqual({
      bg: 'bg-status-published-bg',
      fg: 'text-status-published-fg',
      label: 'Approved',
    })
  })

  it('maps rejected to the changes-requested tokens', () => {
    expect(getApplicationStatusColors('rejected')).toEqual({
      bg: 'bg-status-changes-bg',
      fg: 'text-status-changes-fg',
      label: 'Rejected',
    })
  })
})
