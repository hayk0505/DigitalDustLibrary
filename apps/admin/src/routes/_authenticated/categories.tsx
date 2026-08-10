import { createFileRoute, redirect } from '@tanstack/react-router'
import { getAuthState } from '@/lib/auth-store'
import { canAccessScreen } from '@/hooks/usePermissions'
import { Categories } from '@/pages/Categories'

export const Route = createFileRoute('/_authenticated/categories')({
  beforeLoad: () => {
    if (!canAccessScreen(getAuthState().user?.role ?? null, 'categories')) {
      throw redirect({ to: '/' })
    }
  },
  component: Categories,
})
