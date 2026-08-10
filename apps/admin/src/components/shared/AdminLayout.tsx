import type { ReactNode } from 'react'
import { Link } from '@tanstack/react-router'
import { useAuth } from '@/hooks/useAuth'
import { usePermissions, type Screen } from '@/hooks/usePermissions'
import { useTheme } from '@/hooks/useTheme'
import { usePosts } from '@/lib/api/posts'
import { useApplications } from '@/lib/api/applications'
import { Avatar } from './Avatar'
import { cn } from '@/lib/utils'

const NAV_ITEMS: { screen: Screen; label: string; to: string }[] = [
  { screen: 'dashboard', label: 'Dashboard', to: '/' },
  { screen: 'myPosts', label: 'My Posts', to: '/posts' },
  { screen: 'reviewQueue', label: 'Review Queue', to: '/review' },
  { screen: 'mediaLibrary', label: 'Media Library', to: '/media' },
  { screen: 'categories', label: 'Categories', to: '/categories' },
  { screen: 'applications', label: 'Applications', to: '/applications' },
  { screen: 'usersRoles', label: 'Users & Roles', to: '/users' },
  { screen: 'settings', label: 'Settings', to: '/settings' },
]

export function AdminLayout({ children }: { children: ReactNode }) {
  const { user, logout } = useAuth()
  const { can } = usePermissions()
  const { theme, toggle } = useTheme()

  const canSeeReviewQueue = can('reviewQueue')
  const canSeeApplications = can('applications')
  const { data: posts = [] } = usePosts(undefined, { enabled: canSeeReviewQueue })
  const { data: applications = [] } = useApplications({ enabled: canSeeApplications })

  const badgeCounts: Partial<Record<Screen, number>> = {
    reviewQueue: posts.filter((p) => p.status === 'pending_review').length,
    applications: applications.filter((a) => a.status === 'pending').length,
  }

  return (
    <div className="flex min-h-screen bg-background">
      <aside className="flex w-64 flex-col justify-between bg-sidebar p-6 text-sidebar-foreground">
        <div>
          <div className="mb-8 size-10 rounded-xl bg-sidebar-primary" />
          <nav className="space-y-1">
            {NAV_ITEMS.filter((item) => can(item.screen)).map((item) => {
              const count = badgeCounts[item.screen]
              return (
                <Link
                  key={item.to}
                  to={item.to}
                  className="flex items-center justify-between rounded-lg px-3 py-2 text-sm hover:bg-sidebar-accent"
                  activeProps={{ className: 'bg-sidebar-accent font-medium' }}
                >
                  <span>{item.label}</span>
                  {!!count && (
                    <span className="inline-flex min-w-5 items-center justify-center rounded-full bg-primary px-1.5 text-xs text-primary-foreground">
                      {count}
                    </span>
                  )}
                </Link>
              )
            })}
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
    </div>
  )
}
