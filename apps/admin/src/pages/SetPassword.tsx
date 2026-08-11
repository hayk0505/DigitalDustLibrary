import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useNavigate } from '@tanstack/react-router'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { useAcceptInvite } from '@/lib/api/useAcceptInvite'
import { ApiError } from '@/lib/api/client'

export const setPasswordSchema = z
  .object({
    password: z.string().min(8, 'Password must be at least 8 characters'),
    confirmPassword: z.string(),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: 'Passwords do not match',
    path: ['confirmPassword'],
  })

type SetPasswordForm = z.infer<typeof setPasswordSchema>

export function acceptInviteErrorMessage(error: unknown): string {
  return error instanceof ApiError ? error.message : 'Something went wrong. Please try again.'
}

export function SetPassword({ token }: { token: string }) {
  const navigate = useNavigate()
  const acceptInvite = useAcceptInvite()
  const { register, handleSubmit, formState: { errors } } = useForm<SetPasswordForm>({
    resolver: zodResolver(setPasswordSchema),
  })

  function onSubmit(values: SetPasswordForm) {
    acceptInvite.mutate(
      { token, password: values.password },
      { onSuccess: () => navigate({ to: '/' }) },
    )
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-background">
      <form onSubmit={handleSubmit(onSubmit)} className="w-full max-w-sm space-y-4 rounded-2xl border border-border bg-card p-8 shadow-elevated">
        <h1 className="font-heading text-xl text-foreground">Set your password</h1>

        <div className="space-y-2">
          <Label htmlFor="password">Password</Label>
          <Input id="password" type="password" {...register('password')} />
          {errors.password && <p className="text-sm text-destructive">{errors.password.message}</p>}
        </div>

        <div className="space-y-2">
          <Label htmlFor="confirmPassword">Confirm password</Label>
          <Input id="confirmPassword" type="password" {...register('confirmPassword')} />
          {errors.confirmPassword && (
            <p className="text-sm text-destructive">{errors.confirmPassword.message}</p>
          )}
        </div>

        {acceptInvite.isError && (
          <p className="text-sm text-destructive">{acceptInviteErrorMessage(acceptInvite.error)}</p>
        )}

        <Button type="submit" className="w-full" disabled={acceptInvite.isPending}>
          {acceptInvite.isPending ? 'Setting password…' : 'Set password'}
        </Button>
      </form>
    </div>
  )
}
