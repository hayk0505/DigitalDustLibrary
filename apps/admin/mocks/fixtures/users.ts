import type { Role, User } from '@/lib/types'

export interface SeedAccount {
  user: User
  password: string
}

export const seedAccounts: SeedAccount[] = [
  {
    user: { id: 'user-author', name: 'Alex Rivera', email: 'author@dd.local', role: 'author' },
    password: 'password',
  },
  {
    user: { id: 'user-editor', name: 'Jordan Blake', email: 'editor@dd.local', role: 'editor' },
    password: 'password',
  },
  {
    user: { id: 'user-owner', name: 'Hayk Baroyan', email: 'owner@dd.local', role: 'owner' },
    password: 'password',
  },
]

export function findAccountByEmail(email: string): SeedAccount | undefined {
  return seedAccounts.find((a) => a.user.email === email)
}

export function findUserById(id: string): User | undefined {
  return seedAccounts.find((a) => a.user.id === id)?.user
}

export function encodeMockToken(user: User): string {
  return `mock.${btoa(JSON.stringify({ sub: user.id, role: user.role }))}`
}

export function decodeMockToken(token: string): { sub: string; role: Role } | null {
  const [prefix, payload] = token.split('.')
  if (prefix !== 'mock' || !payload) return null
  try {
    return JSON.parse(atob(payload))
  } catch {
    return null
  }
}
