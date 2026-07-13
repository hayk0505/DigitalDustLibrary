import { describe, expect, it } from 'vitest'
import { render, screen } from '@testing-library/react'
import { PillarTag } from './PillarTag'

describe('PillarTag', () => {
  it('renders the pillar label', () => {
    render(<PillarTag pillar="tech" />)
    expect(screen.getByText('Tech')).toBeInTheDocument()
  })

  it('renders a colored dot using the pillar token class', () => {
    render(<PillarTag pillar="software_dev" />)
    const dot = screen.getByTestId('pillar-dot')
    expect(dot.className).toContain('bg-pillar-dev')
  })
})
