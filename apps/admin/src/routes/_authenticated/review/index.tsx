import { createFileRoute, redirect } from '@tanstack/react-router'
import { getAuthState } from '@/lib/auth-store'
import { canAccessScreen } from '@/hooks/usePermissions'
import { ReviewQueue } from '@/pages/ReviewQueue'

export const Route = createFileRoute('/_authenticated/review/')({
  beforeLoad: () => {
    if (!canAccessScreen(getAuthState().user?.role ?? null, 'reviewQueue')) {
      throw redirect({ to: '/' })
    }
  },
  component: ReviewQueue,
})
