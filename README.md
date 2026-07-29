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

Pre-launch. The public blog and admin panel are both in active development; the
`.NET` API they'll both call doesn't exist yet, so the admin panel currently runs
against a mocked backend (MSW).

## Structure

```
apps/
  blog/    SvelteKit public-facing blog (Svelte 5, Tailwind CSS 4)
  admin/   React authoring/moderation panel (React 19, TanStack Router/Query, shadcn/ui)
```

`apps/api` (ASP.NET Core Web API + PostgreSQL) is planned but not yet scaffolded —
see the tech stack notes in [CLAUDE.md](CLAUDE.md) for the intended shape
(auth, multi-author workflow, etc.).

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
