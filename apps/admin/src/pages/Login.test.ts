import { describe, expect, it } from 'vitest'
import { ApiError } from '@/lib/api/client'
import { loginErrorMessage } from './Login'

describe('loginErrorMessage', () => {
  it('returns the server message for an ApiError', () => {
    expect(loginErrorMessage(new ApiError(429, 'Too many attempts, try again in 5 minutes'))).toBe(
      'Too many attempts, try again in 5 minutes',
    )
  })

  it('falls back to a generic message for a non-ApiError error', () => {
    expect(loginErrorMessage(new TypeError('Failed to fetch'))).toBe('Invalid email or password.')
  })
})
