import { createFileRoute, redirect } from '@tanstack/react-router'
import { getAuthState } from '@/lib/auth-store'
import { canAccessScreen } from '@/hooks/usePermissions'
import { Settings } from '@/pages/Settings'

export const Route = createFileRoute('/_authenticated/settings')({
  beforeLoad: () => {
    if (!canAccessScreen(getAuthState().user?.role ?? null, 'settings')) {
      throw redirect({ to: '/' })
    }
  },
  component: Settings,
})
