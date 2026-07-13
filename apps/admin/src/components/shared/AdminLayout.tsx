import type { ReactNode } from 'react'
import { Link } from '@tanstack/react-router'
import { Toaster } from '@/components/ui/sonner'
import { useAuth } from '@/hooks/useAuth'
import { usePermissions, type Screen } from '@/hooks/usePermissions'
import { useTheme } from '@/hooks/useTheme'
import { Avatar } from './Avatar'
import { cn } from '@/lib/utils'

const NAV_ITEMS: { screen: Screen; label: string; to: string }[] = [
  { screen: 'dashboard', label: 'Dashboard', to: '/' },
  { screen: 'myPosts', label: 'My Posts', to: '/posts' },
  { screen: 'mediaLibrary', label: 'Media Library', to: '/media' },
]

export function AdminLayout({ children }: { children: ReactNode }) {
  const { user, logout } = useAuth()
  const { can } = usePermissions()
  const { theme, toggle } = useTheme()

  return (
    <div className="flex min-h-screen bg-background">
      <aside className="flex w-64 flex-col justify-between bg-sidebar p-6 text-sidebar-foreground">
        <div>
          <div className="mb-8 size-10 rounded-xl bg-sidebar-primary" />
          <nav className="space-y-1">
            {NAV_ITEMS.filter((item) => can(item.screen)).map((item) => (
              <Link
                key={item.to}
                to={item.to}
                className="block rounded-lg px-3 py-2 text-sm hover:bg-sidebar-accent"
                activeProps={{ className: 'bg-sidebar-accent font-medium' }}
              >
                {item.label}
              </Link>
            ))}
          </nav>
        </div>

        <div className="space-y-3">
          <button
            type="button"
            onClick={toggle}
            className="w-full rounded-lg px-3 py-2 text-left text-sm hover:bg-sidebar-accent"
          >
            {theme === 'dark' ? 'Switch to light' : 'Switch to dark'}
          </button>
          {user && (
            <div className="flex items-center gap-2">
              <Avatar name={user.name} size="sm" />
              <div className="min-w-0">
                <p className="truncate text-sm">{user.name}</p>
                <button type="button" onClick={logout} className="text-xs text-sidebar-foreground/70 hover:underline">
                  Sign out
                </button>
              </div>
            </div>
          )}
        </div>
      </aside>

      <main className={cn('flex-1 p-8')}>{children}</main>
      <Toaster />
    </div>
  )
}
