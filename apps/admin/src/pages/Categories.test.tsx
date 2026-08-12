import { describe, expect, it } from 'vitest'
import { categorySchema } from './Categories'

describe('categorySchema', () => {
  const base = {
    name: 'Tech',
    slug: 'tech',
    description: 'Where the industry\'s tools get taken apart.',
    color: '#C9553D',
  }

  it('leaves position undefined when submitted blank (an untouched number input reads back as "")', () => {
    const result = categorySchema.safeParse({ ...base, position: '' })
    expect(result.success).toBe(true)
    if (result.success) {
      expect(result.data.position).toBeUndefined()
    }
  })

  it('leaves position undefined when omitted entirely', () => {
    const result = categorySchema.safeParse({ ...base })
    expect(result.success).toBe(true)
    if (result.success) {
      expect(result.data.position).toBeUndefined()
    }
  })

  it('still coerces a real numeric string to a number', () => {
    const result = categorySchema.safeParse({ ...base, position: '42' })
    expect(result.success).toBe(true)
    if (result.success) {
      expect(result.data.position).toBe(42)
    }
  })

  it('rejects a hex color missing the leading #', () => {
    const result = categorySchema.safeParse({ ...base, color: 'C9553D' })
    expect(result.success).toBe(false)
  })
})
