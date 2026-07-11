# Monorepo Scaffold — Blog + Admin (design)

Date: 2026-07-11

## Purpose

Stand up the Turborepo monorepo skeleton for Digital Dust Library, per the tech
stack already decided in `CLAUDE.md`. This pass creates the two frontend apps
only — `apps/api` and all `packages/*` are explicitly deferred until there's
real shared code / an API to generate types from.

## Architecture

- pnpm workspaces: `pnpm-workspace.yaml` with `apps/*` and `packages/*`.
- Turborepo: root `turbo.json` defining `dev`, `build`, `lint`, `check`
  pipeline tasks that fan out to each app via `turbo run <task>`.
- Root `package.json`: private, `turbo` as a devDependency, root scripts
  wrapping `turbo run <task>`.
- `.gitignore` extended to cover `node_modules`, `.turbo`, `dist`, `build`,
  `.svelte-kit`, `.env*`.
- No shared `packages/*` yet — each app is self-contained for now.
- No `apps/api` yet — that's a separate future pass (.NET 10 Web API).

## Components

**`apps/blog`** — public-facing site (see `CLAUDE.md` for the long-term
Cloudflare/Tailwind/SEO plan, none of which is wired up in this pass).
- Scaffolded via `npx sv create` (SvelteKit CLI).
- Options: minimal template, TypeScript, ESLint add-on, Prettier add-on.
- Vitest and Playwright explicitly skipped — nothing to test yet.

**`apps/admin`** — authoring/moderation SPA (see `CLAUDE.md` for the
long-term shadcn/ui/TipTap plan, none of which is wired up in this pass).
- Scaffolded via `pnpm create vite@latest` with the `react-ts` template.
- ESLint ships with this template by default; no extra add-on step needed.

## Explicitly deferred (not part of this pass)

- `apps/api` (.NET 10 ASP.NET Core Web API) — no .NET files created.
- `packages/shared-types` — nothing to generate from without `apps/api`.
- `packages/shared-utils`, `packages/validation` — no shared code yet.
- Tailwind CSS, `@tailwindcss/typography` — not installed in `apps/blog`.
- shadcn/ui, TipTap — not installed in `apps/admin`.

## Data flow / error handling

Not applicable to this pass — both apps are unmodified generator output with
no business logic, no API calls, and no custom routes beyond the scaffolder
defaults.

## Verification

- `pnpm install` at the repo root resolves both workspace apps with no errors.
- `pnpm --filter blog build` succeeds.
- `pnpm --filter admin build` succeeds.
- `pnpm --filter blog dev` and `pnpm --filter admin dev` each boot and serve
  without error (smoke check — no custom UI exists yet to walk through in a
  browser, so this is a "does it start" check, not a feature walkthrough).

## Out of scope / open questions carried forward

- Everything under "Explicitly deferred" above becomes its own future spec
  when picked up (API pass, shared packages pass, styling/component-library
  pass).
