# Digital Dust Library

Long-form blog for Hayk Baroyan (`digitaldustlibrary.com`, reserved — not yet live).
Three content pillars: **Tech**, **Social/Psychological**, and **Software Development**,
all deep-dive length. Long-form pieces published here also feed the
[Glitch](https://www.youtube.com/) YouTube pipeline as source research/writing, and
short LinkedIn posts drive traffic in.

This repo is a pnpm/Turborepo monorepo containing the blog frontend and its admin
authoring panel. It does **not** contain the separate Ryan Kobary / Glitch fiction
universe material — see [CLAUDE.md](CLAUDE.md) for the full identity-separation
rationale before adding content here.

## Status

Pre-launch. The public blog and admin panel are both in active development.
`apps/api` (.NET 10 + PostgreSQL) now exists as a scaffold covering auth, posts,
and media — matching `apps/admin`'s Phase 1 exactly — but hasn't been built/tested
against a real .NET SDK yet, and doesn't yet cover Categories, Author
Applications, Users & Roles, or Statistics. The admin panel still defaults to a
mocked backend (MSW) for now; see `apps/api/README.md` for how to point it at
the real API instead.

## Structure

```
apps/
  blog/    SvelteKit public-facing blog (Svelte 5, Tailwind CSS 4)
  admin/   React authoring/moderation panel (React 19, TanStack Router/Query, shadcn/ui)
  api/     ASP.NET Core (.NET 10) Web API + PostgreSQL 18 — scaffolded, partial (see apps/api/README.md)
```

Local dev for the full stack (API + Postgres) runs via Docker Compose, kept
separate from Turborepo (which stays scoped to the JS/TS apps):

```bash
docker compose -f docker-compose.dev.yml up
```

See the tech stack notes in [CLAUDE.md](CLAUDE.md) for the intended full shape
(auth, multi-author workflow, etc.) and [Admin_Panel_Build_Spec.md](Admin_Panel_Build_Spec.md)
for the screen-by-screen contract this API is built against.

## Getting started

Requires [pnpm](https://pnpm.io/) and Node.js 24+.

```bash
pnpm install

# run everything
pnpm dev

# or run one app at a time
pnpm --filter blog dev
pnpm --filter admin dev
```

Other root-level scripts (fan out to every package via Turborepo):

```bash
pnpm build
pnpm lint
pnpm check
```

Each app has its own README with app-specific details:
[apps/blog](apps/blog/README.md) · [apps/admin](apps/admin/README.md)

## Documentation

- [CLAUDE.md](CLAUDE.md) — project memory: architecture decisions, tech stack,
  hosting plan, identity-separation rules, and open decisions.
- [Content_Ecosystem_Structure.md](Content_Ecosystem_Structure.md) — how this blog
  fits alongside LinkedIn, Glitch, and the other identities in the wider content
  ecosystem.
- [Functional_Overview_for_Design.md](Functional_Overview_for_Design.md) — screen-by-
  screen functional brief for the public blog and admin panel.
- [Admin_Panel_Build_Spec.md](Admin_Panel_Build_Spec.md) — detailed build spec for
  the admin panel.
- `docs/superpowers/` — dated plans and specs written while building each feature.

## Conventions

- Article drafts live as `.docx` files, one subfolder per topic — not tracked in the
  code review flow.
- Office lock/temp files (`~$*.docx`, `~WRL*.tmp`) are gitignored; never commit these.
