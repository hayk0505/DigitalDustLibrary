import { apiFetch } from './client'
import type { AuthResponse } from '@/lib/types'

export function login(email: string, password: string): Promise<AuthResponse> {
  return apiFetch<AuthResponse>('/auth/login', {
    method: 'POST',
    body: JSON.stringify({ email, password }),
  })
}

export function logout(): Promise<void> {
  return apiFetch<void>('/auth/logout', { method: 'POST' })
}

export function acceptInvite(token: string, password: string): Promise<AuthResponse> {
  return apiFetch<AuthResponse>('/auth/accept-invite', {
    method: 'POST',
    body: JSON.stringify({ token, password }),
  })
}
