import { describe, expect, it, beforeEach, afterEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { vi } from 'vitest'
import { setAuthState } from '@/lib/auth-store'
import { AdminLayout } from './AdminLayout'

vi.mock('@tanstack/react-router', () => ({
  useNavigate: () => vi.fn(),
  Link: ({ children, className, to }: { children: React.ReactNode; className?: string; to?: string }) => (
    <a href={to} className={className}>
      {children}
    </a>
  ),
}))

function renderLayout() {
  setAuthState({
    accessToken: 'tok',
    user: { id: 'author-1', name: 'Alex', email: 'alex@dd.local', role: 'author' },
  })
  const queryClient = new QueryClient()
  return render(
    <QueryClientProvider client={queryClient}>
      <AdminLayout>content</AdminLayout>
    </QueryClientProvider>,
  )
}

describe('AdminLayout', () => {
  beforeEach(() => {
    localStorage.clear()
    document.documentElement.classList.remove('dark')
  })

  afterEach(() => {
    setAuthState({ accessToken: null, user: null })
  })

  it('renders a clickable-looking Sign out control', () => {
    renderLayout()

    const signOut = screen.getByRole('button', { name: 'Sign out' })
    expect(signOut.className).toContain('cursor-pointer')
  })

  it('renders a theme switch that reflects and toggles the current theme', () => {
    renderLayout()

    const themeSwitch = screen.getByRole('switch')
    expect(themeSwitch).toHaveAttribute('aria-checked', 'false')

    fireEvent.click(themeSwitch)

    expect(themeSwitch).toHaveAttribute('aria-checked', 'true')
    expect(document.documentElement.classList.contains('dark')).toBe(true)
  })

  // Regression test: the outer shell used to be `min-h-screen`, which only
  // sets a floor, not a cap — a main content area taller than the viewport
  // (e.g. a long Users & Roles table) grew the whole page past 100vh, so
  // the browser scrolled the entire document and dragged the sidebar's
  // bottom section (username, theme toggle) off-screen with it, since
  // nothing kept the sidebar pinned to the viewport independently. The
  // shell must be capped to exactly the viewport height with its own
  // scroll containment, so only <main> scrolls internally.
  it('pins the shell to exactly the viewport height so only the main content scrolls', () => {
    const { container } = renderLayout()

    const shell = container.firstElementChild as HTMLElement
    expect(shell.className).toContain('h-screen')
    expect(shell.className).toContain('overflow-hidden')

    const main = screen.getByRole('main')
    expect(main.className).toContain('overflow-y-auto')
  })

  // Regression test: the sidebar stays dark-styled in both themes (see
  // index.css's --sidebar tokens — it doesn't flip light like the main
  // content area does), but the theme switch originally used the Switch
  // component's default bg-input/bg-background colors, which ARE tuned to
  // flip with the theme. In light mode that resolved to a white track
  // (--input) with a near-white thumb (--background) — both nearly
  // invisible against each other and hard to read against the sidebar.
  // The switch needs sidebar-scoped colors instead, so it stays legible
  // regardless of which overall theme is active.
  it('uses sidebar-scoped colors for the theme switch, not the theme-dependent defaults', () => {
    renderLayout()

    const themeSwitch = screen.getByRole('switch')
    expect(themeSwitch.className).not.toContain('bg-input')
    expect(themeSwitch.className).toContain('bg-sidebar-foreground')
    expect(themeSwitch.className).toContain('bg-sidebar-primary')
  })
})
