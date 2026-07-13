import { afterEach, describe, expect, it } from 'vitest'
import { renderHook } from '@testing-library/react'
import { setAuthState } from '@/lib/auth-store'
import { usePermissions } from './usePermissions'

afterEach(() => {
  setAuthState({ accessToken: null, user: null })
})

describe('usePermissions', () => {
  it('returns null role and denies everything when logged out', () => {
    const { result } = renderHook(() => usePermissions())
    expect(result.current.role).toBeNull()
    expect(result.current.can('dashboard')).toBe(false)
  })

  it('allows an author to see all-roles screens but not editor/owner screens', () => {
    setAuthState({ accessToken: 't', user: { id: '1', name: 'A', email: 'a@dd.local', role: 'author' } })
    const { result } = renderHook(() => usePermissions())
    expect(result.current.can('dashboard')).toBe(true)
    expect(result.current.can('mediaLibrary')).toBe(true)
    expect(result.current.can('reviewQueue')).toBe(false)
    expect(result.current.can('usersRoles')).toBe(false)
  })

  it('allows an editor to see reviewQueue but not usersRoles', () => {
    setAuthState({ accessToken: 't', user: { id: '2', name: 'E', email: 'e@dd.local', role: 'editor' } })
    const { result } = renderHook(() => usePermissions())
    expect(result.current.can('reviewQueue')).toBe(true)
    expect(result.current.can('applications')).toBe(true)
    expect(result.current.can('usersRoles')).toBe(false)
  })

  it('allows an owner to see every screen', () => {
    setAuthState({ accessToken: 't', user: { id: '3', name: 'O', email: 'o@dd.local', role: 'owner' } })
    const { result } = renderHook(() => usePermissions())
    expect(result.current.can('usersRoles')).toBe(true)
    expect(result.current.can('settings')).toBe(true)
  })
})
