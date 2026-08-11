import { describe, expect, it } from 'vitest'
import { render, screen } from '@testing-library/react'
import { setPasswordSearchSchema, SetPasswordRouteView } from './set-password'

describe('setPasswordSearchSchema', () => {
  it('keeps a valid token string', () => {
    expect(setPasswordSearchSchema.parse({ token: 'abc123' })).toEqual({ token: 'abc123' })
  })

  it('falls back to undefined for a non-string token instead of throwing', () => {
    expect(setPasswordSearchSchema.parse({ token: 123 })).toEqual({ token: undefined })
  })

  it('falls back to undefined when token is absent', () => {
    expect(setPasswordSearchSchema.parse({})).toEqual({ token: undefined })
  })
})

describe('SetPasswordRouteView', () => {
  it('shows an invalid-link message when no token is present', () => {
    render(<SetPasswordRouteView token={undefined} />)
    expect(screen.getByText('This invite link is invalid.')).toBeInTheDocument()
  })
})
