import { describe, expect, it } from 'vitest'
import { render, screen } from '@testing-library/react'
import { StatTile } from './StatTile'

describe('StatTile', () => {
  it('renders the label and value', () => {
    render(<StatTile label="Drafts" value={4} />)
    expect(screen.getByText('Drafts')).toBeInTheDocument()
    expect(screen.getByText('4')).toBeInTheDocument()
  })

  it('renders a trend delta when provided', () => {
    render(<StatTile label="Views (30d)" value="1.2k" trend={{ direction: 'up', value: '+12%' }} />)
    expect(screen.getByText('+12%')).toBeInTheDocument()
  })

  it('omits the trend element when not provided', () => {
    render(<StatTile label="Drafts" value={4} />)
    expect(screen.queryByTestId('stat-trend')).not.toBeInTheDocument()
  })
})
