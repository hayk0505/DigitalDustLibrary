import { describe, expect, it } from 'vitest'
import { render, screen } from '@testing-library/react'
import { StatusBadge } from './StatusBadge'

describe('StatusBadge', () => {
  it('renders the label for a status', () => {
    render(<StatusBadge status="pending_review" />)
    expect(screen.getByText('Pending Review')).toBeInTheDocument()
  })

  it('applies the token classes for the status', () => {
    render(<StatusBadge status="published" />)
    const badge = screen.getByText('Published')
    expect(badge.className).toContain('bg-status-published-bg')
    expect(badge.className).toContain('text-status-published-fg')
  })
})
