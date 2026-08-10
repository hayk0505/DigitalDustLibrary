import { createFileRoute, redirect } from '@tanstack/react-router'
import { getAuthState } from '@/lib/auth-store'
import { canAccessScreen } from '@/hooks/usePermissions'
import { Applications } from '@/pages/Applications'

export const Route = createFileRoute('/_authenticated/applications')({
  beforeLoad: () => {
    if (!canAccessScreen(getAuthState().user?.role ?? null, 'applications')) {
      throw redirect({ to: '/' })
    }
  },
  component: Applications,
})
