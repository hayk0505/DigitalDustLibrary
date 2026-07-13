import { useSyncExternalStore } from 'react'
import { getAuthState, setAuthState, subscribeAuth } from '@/lib/auth-store'

export function useAuth() {
  const state = useSyncExternalStore(subscribeAuth, getAuthState)

  function logout() {
    setAuthState({ accessToken: null, user: null })
  }

  return { user: state.user, accessToken: state.accessToken, logout }
}
