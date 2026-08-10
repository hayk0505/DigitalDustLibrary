import { useAuth } from './useAuth'
import type { Role } from '@/lib/types'

export type Screen =
  | 'dashboard'
  | 'myPosts'
  | 'postEditor'
  | 'mediaLibrary'
  | 'statistics'
  | 'reviewQueue'
  | 'applications'
  | 'categories'
  | 'usersRoles'
  | 'settings'

const SCREEN_ROLES: Record<Screen, Role[]> = {
  dashboard: ['author', 'editor', 'owner'],
  myPosts: ['author', 'editor', 'owner'],
  postEditor: ['author', 'editor', 'owner'],
  mediaLibrary: ['author', 'editor', 'owner'],
  statistics: ['author', 'editor', 'owner'],
  reviewQueue: ['editor', 'owner'],
  applications: ['editor', 'owner'],
  categories: ['editor', 'owner'],
  usersRoles: ['owner'],
  settings: ['owner'],
}

export function canAccessScreen(role: Role | null, screen: Screen): boolean {
  if (!role) return false
  return SCREEN_ROLES[screen].includes(role)
}

export function usePermissions() {
  const { user } = useAuth()
  const role = user?.role ?? null

  return { role, can: (screen: Screen) => canAccessScreen(role, screen) }
}
