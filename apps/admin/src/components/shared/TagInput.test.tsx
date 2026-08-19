import { describe, expect, it } from 'vitest'
import { filterSuggestions, shouldOfferCreate, type TagOption } from './TagInput'

const existing: TagOption[] = [
  { id: 'tag-ai', name: 'AI' },
  { id: 'tag-internet-culture', name: 'Internet Culture' },
]

describe('filterSuggestions', () => {
  it('returns nothing for an empty query', () => {
    expect(filterSuggestions('', existing, [])).toEqual([])
  })

  it('matches case-insensitively on a substring', () => {
    expect(filterSuggestions('ai', existing, [])).toEqual([existing[0]])
  })

  it('excludes tags already selected', () => {
    expect(filterSuggestions('ai', existing, [existing[0]])).toEqual([])
  })
})

describe('shouldOfferCreate', () => {
  it('is false for an empty query', () => {
    expect(shouldOfferCreate('', existing)).toBe(false)
  })

  it('is false when an exact case-insensitive match already exists', () => {
    expect(shouldOfferCreate('ai', existing)).toBe(false)
  })

  it('is true for a genuinely new tag name', () => {
    expect(shouldOfferCreate('Robotics', existing)).toBe(true)
  })
})
