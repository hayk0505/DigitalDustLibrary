import { describe, expect, it } from 'vitest'
import { render, screen } from '@testing-library/react'
import { Avatar } from './Avatar'

describe('Avatar', () => {
  it('renders initials from the name', () => {
    render(<Avatar name="Alex Rivera" />)
    expect(screen.getByText('AR')).toBeInTheDocument()
  })

  it('applies the avatar token classes', () => {
    render(<Avatar name="Alex Rivera" />)
    const el = screen.getByText('AR')
    expect(el.className).toContain('bg-avatar-bg')
    expect(el.className).toContain('text-avatar-fg')
  })

  it('applies a larger size class when size="lg"', () => {
    render(<Avatar name="Alex Rivera" size="lg" />)
    const el = screen.getByText('AR')
    expect(el.className).toContain('size-12')
  })
})
