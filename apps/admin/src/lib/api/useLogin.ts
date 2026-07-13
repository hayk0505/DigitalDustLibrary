import { useMutation } from '@tanstack/react-query'
import { login } from './auth'
import { setAuthState } from '@/lib/auth-store'

export function useLogin() {
  return useMutation({
    mutationFn: ({ email, password }: { email: string; password: string }) => login(email, password),
    onSuccess: (data) => {
      setAuthState({ accessToken: data.accessToken, user: data.user })
    },
  })
}
