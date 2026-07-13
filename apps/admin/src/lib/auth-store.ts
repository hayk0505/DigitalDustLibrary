import type { User } from './types'

interface AuthState {
  accessToken: string | null
  user: User | null
}

let state: AuthState = { accessToken: null, user: null }
const listeners = new Set<() => void>()

export function getAuthState(): AuthState {
  return state
}

export function setAuthState(next: AuthState): void {
  state = next
  listeners.forEach((listener) => listener())
}

export function subscribeAuth(callback: () => void): () => void {
  listeners.add(callback)
  return () => listeners.delete(callback)
}
