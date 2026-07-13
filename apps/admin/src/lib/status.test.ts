import { describe, expect, it } from 'vitest'
import { getPillarColor, getStatusColors } from './status'

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

describe('getPillarColor', () => {
  it('maps tech to its token class and label', () => {
    expect(getPillarColor('tech')).toEqual({ bg: 'bg-pillar-tech', label: 'Tech' })
  })

  it('maps software_dev to its token class and label', () => {
    expect(getPillarColor('software_dev')).toEqual({
      bg: 'bg-pillar-dev',
      label: 'Software Development',
    })
  })
})
