import { createFileRoute, redirect } from '@tanstack/react-router'
import { getAuthState } from '@/lib/auth-store'
import { canAccessScreen } from '@/hooks/usePermissions'
import { ReviewDetail } from '@/pages/ReviewDetail'

export const Route = createFileRoute('/_authenticated/review/$postId')({
  beforeLoad: () => {
    if (!canAccessScreen(getAuthState().user?.role ?? null, 'reviewQueue')) {
      throw redirect({ to: '/' })
    }
  },
  component: ReviewDetailRoute,
})

function ReviewDetailRoute() {
  const { postId } = Route.useParams()
  return <ReviewDetail postId={postId} />
}
