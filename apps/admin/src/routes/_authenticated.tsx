import { createFileRoute, Outlet, redirect } from '@tanstack/react-router'
import { getAuthState } from '@/lib/auth-store'
import { AdminLayout } from '@/components/shared/AdminLayout'

export const Route = createFileRoute('/_authenticated')({
  beforeLoad: () => {
    if (!getAuthState().user) {
      throw redirect({ to: '/login' })
    }
  },
  component: () => (
    <AdminLayout>
      <Outlet />
    </AdminLayout>
  ),
})
