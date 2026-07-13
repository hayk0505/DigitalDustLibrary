# Digital Dust Library — Admin

Authoring/moderation panel for Digital Dust Library. React 19 + TanStack Router/Query
+ Tailwind v4 + shadcn/ui. See `Admin_Panel_Build_Spec.md` (repo root) for the full
functional spec and `docs/superpowers/specs/2026-07-13-admin-panel-phase1-design.md`
for what's implemented so far.

## Running locally

```bash
pnpm --filter admin dev
```

This phase runs entirely against a mocked backend (MSW) — `apps/api` doesn't exist
yet. Mocks are enabled via `VITE_ENABLE_MOCKS=true` in `.env.development`.

## Mock accounts

| Email | Password | Role |
|---|---|---|
| `author@dd.local` | `password` | Author |
| `editor@dd.local` | `password` | Editor |
| `owner@dd.local` | `password` | Owner |

All three roles currently see the same screens (Dashboard, My Posts, Post Editor,
Media Library) — Editor/Owner-only screens (Review Queue, Applications, Categories,
Users & Roles, Settings) land in a later phase.

## Testing

```bash
pnpm --filter admin test        # run once
pnpm --filter admin test:watch  # watch mode
```
