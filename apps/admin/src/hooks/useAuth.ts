import { useSyncExternalStore } from 'react'
import { useNavigate } from '@tanstack/react-router'
import { getAuthState, setAuthState, subscribeAuth } from '@/lib/auth-store'
import { logout as logoutRequest } from '@/lib/api/auth'

export function useAuth() {
  const state = useSyncExternalStore(subscribeAuth, getAuthState)
  const navigate = useNavigate()

  function logout() {
    setAuthState({ accessToken: null, user: null })
    logoutRequest().catch(() => {
      // best-effort — if this fails, the refresh cookie/token still expires naturally
    })
    navigate({ to: '/login' })
  }

  return { user: state.user, accessToken: state.accessToken, logout }
}
