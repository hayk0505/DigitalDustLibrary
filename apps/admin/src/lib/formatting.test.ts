import { describe, expect, it, vi, beforeEach, afterEach } from 'vitest'
import { formatRelativeTime, getInitials } from './formatting'

beforeEach(() => {
  vi.useFakeTimers()
})

afterEach(() => {
  vi.useRealTimers()
})

describe('getInitials', () => {
  it('returns the first letter of the first two words', () => {
    expect(getInitials('Alex Rivera')).toBe('AR')
  })

  it('handles a single name', () => {
    expect(getInitials('Alex')).toBe('A')
  })

  it('ignores extra whitespace', () => {
    expect(getInitials('  Alex   Rivera  ')).toBe('AR')
  })
})

describe('formatRelativeTime', () => {
  it('formats minutes ago', () => {
    vi.setSystemTime(new Date('2026-07-13T12:00:00Z'))
    expect(formatRelativeTime(new Date('2026-07-13T11:55:00Z'))).toBe('5m ago')
  })

  it('formats hours ago', () => {
    vi.setSystemTime(new Date('2026-07-13T12:00:00Z'))
    expect(formatRelativeTime(new Date('2026-07-13T09:00:00Z'))).toBe('3h ago')
  })

  it('formats days ago', () => {
    vi.setSystemTime(new Date('2026-07-13T12:00:00Z'))
    expect(formatRelativeTime(new Date('2026-07-10T12:00:00Z'))).toBe('3d ago')
  })
})
