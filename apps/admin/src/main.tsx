import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { QueryClientProvider } from '@tanstack/react-query'
import { createRouter, RouterProvider } from '@tanstack/react-router'
import './index.css'
import { routeTree } from './routeTree.gen'
import { API_BASE_URL } from '@/lib/api/client'
import { queryClient } from '@/lib/queryClient'
import { setAuthState } from '@/lib/auth-store'
import type { User } from '@/lib/types'

const router = createRouter({ routeTree })

declare module '@tanstack/react-router' {
  interface Register {
    router: typeof router
  }
}

async function bootstrap() {
  if (import.meta.env.VITE_ENABLE_MOCKS === 'true') {
    const { worker } = await import('../mocks/browser')
    await worker.start({ onUnhandledRequest: 'bypass' })
    return
  }

  // Real backend: auth state is in-memory only (Admin_Panel_Build_Spec.md's
  // "no localStorage" decision), so a hard refresh otherwise always starts
  // logged-out. Use the httpOnly refresh cookie to silently restore a session
  // before the router's first render, so _authenticated.tsx's guard sees it.
  try {
    const refreshRes = await fetch(`${API_BASE_URL}/auth/refresh`, { method: 'POST' })
    if (!refreshRes.ok) return
    const { accessToken } = (await refreshRes.json()) as { accessToken: string }

    const meRes = await fetch(`${API_BASE_URL}/auth/me`, {
      headers: { Authorization: `Bearer ${accessToken}` },
    })
    if (!meRes.ok) return
    const user = (await meRes.json()) as User
    setAuthState({ accessToken, user })
  } catch {
    // best-effort — proceed logged-out on any failure (no cookie, network error, etc.)
  }
}

bootstrap().then(() => {
  createRoot(document.getElementById('root')!).render(
    <StrictMode>
      <QueryClientProvider client={queryClient}>
        <RouterProvider router={router} />
      </QueryClientProvider>
    </StrictMode>,
  )
})
