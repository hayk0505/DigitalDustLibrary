import { describe, expect, it } from 'vitest'
import { render, screen } from '@testing-library/react'
import { CategoryTag } from './CategoryTag'

describe('CategoryTag', () => {
  it('renders the category name', () => {
    render(<CategoryTag name="Tech" color="#C9553D" />)
    expect(screen.getByText('Tech')).toBeInTheDocument()
  })

  it('renders a dot colored with the category\'s own color', () => {
    render(<CategoryTag name="Software Dev" color="#4A6FBF" />)
    const dot = screen.getByTestId('category-dot')
    expect(dot).toHaveStyle({ backgroundColor: '#4A6FBF' })
  })
})
