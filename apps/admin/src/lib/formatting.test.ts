import { describe, expect, it, vi, beforeEach, afterEach } from 'vitest'
import { estimateReadTime, formatRelativeTime, getInitials, slugify } from './formatting'

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

describe('estimateReadTime', () => {
  it('estimates based on word count at ~200wpm', () => {
    const words = Array(400).fill('word').join(' ')
    expect(estimateReadTime(`<p>${words}</p>`)).toBe('2 min read')
  })

  it('rounds up to at least 1 minute for short content', () => {
    expect(estimateReadTime('<p>Just a few words here.</p>')).toBe('1 min read')
  })

  it('strips HTML tags before counting words', () => {
    expect(estimateReadTime('<h1>Title</h1><p>Some <strong>bold</strong> text.</p>')).toBe('1 min read')
  })
})

describe('slugify', () => {
  it('lowercases and hyphenates spaces', () => {
    expect(slugify('Social & Psychological')).toBe('social-psychological')
  })

  it('collapses consecutive non-alphanumeric characters into one hyphen', () => {
    expect(slugify('Tech  --  Trends!!')).toBe('tech-trends')
  })

  it('trims leading and trailing hyphens', () => {
    expect(slugify('  -Software Dev- ')).toBe('software-dev')
  })
})
