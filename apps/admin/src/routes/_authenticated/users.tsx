import { createFileRoute, redirect } from '@tanstack/react-router'
import { getAuthState } from '@/lib/auth-store'
import { canAccessScreen } from '@/hooks/usePermissions'
import { Users } from '@/pages/Users'

export const Route = createFileRoute('/_authenticated/users')({
  beforeLoad: () => {
    if (!canAccessScreen(getAuthState().user?.role ?? null, 'usersRoles')) {
      throw redirect({ to: '/' })
    }
  },
  component: Users,
})
