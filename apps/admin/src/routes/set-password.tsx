import { createFileRoute } from '@tanstack/react-router'
import { z } from 'zod'
import { SetPassword } from '@/pages/SetPassword'

export const setPasswordSearchSchema = z.object({
  token: z.string().optional().catch(undefined),
})

export const Route = createFileRoute('/set-password')({
  validateSearch: setPasswordSearchSchema,
  component: SetPasswordRoute,
})

export function SetPasswordRouteView({ token }: { token: string | undefined }) {
  if (!token) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-background">
        <p className="text-sm text-destructive">This invite link is invalid.</p>
      </div>
    )
  }

  return <SetPassword token={token} />
}

function SetPasswordRoute() {
  const { token } = Route.useSearch()
  return <SetPasswordRouteView token={token} />
}
