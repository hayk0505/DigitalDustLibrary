import { useMutation } from '@tanstack/react-query'
import { acceptInvite } from './auth'
import { setAuthState } from '@/lib/auth-store'

export function useAcceptInvite() {
  return useMutation({
    mutationFn: ({ token, password }: { token: string; password: string }) =>
      acceptInvite(token, password),
    onSuccess: (data) => {
      setAuthState({ accessToken: data.accessToken, user: data.user })
    },
    meta: { skipErrorToast: true },
  })
}
