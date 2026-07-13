import { createFileRoute, redirect } from '@tanstack/react-router'
import { getAuthState } from '@/lib/auth-store'
import { Login } from '@/pages/Login'

export const Route = createFileRoute('/login')({
  beforeLoad: () => {
    if (getAuthState().user) {
      throw redirect({ to: '/' })
    }
  },
  component: Login,
})
