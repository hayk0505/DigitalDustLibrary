# Digital Dust Library — Project Memory

This file gives context for anyone (including a future Claude session) picking up this
repo cold. Read this before making structural changes.

## What this is

Digital Dust Library is Hayk Baroyan's long-form blog. Domain: `digitaldustlibrary.com`
(confirmed available, not yet live). It's one piece of a wider content ecosystem — this
repo holds only the blog itself, not the other pieces.

Three content pillars, all deep-dive length:
- Tech
- Social / psychological
- Software development

Role in the pipeline: source material for Glitch (YouTube) video scripts. Short posts on
LinkedIn drive traffic in; long-form lives here; Glitch pulls research/writing from here
rather than starting from scratch per video.

## Identity separation — read this before adding content

This is the most important structural rule in the whole ecosystem, so it's worth stating
plainly: **Digital Dust Library / Hayk Baroyan is deliberately kept separate from the
Ryan Kobary / Glitch fiction universe.**

- Hayk Baroyan = real name, engineer identity, CV/projects site (`haykbaroyan.com`),
  LinkedIn, and this blog.
- Ryan Kobary = pen name for the Glitch fiction/world-building universe (telepathic
  races, retro-digital-mystery setting). Own domain (`ryankobary.com` or similar, TBD).
  Lives in the parent `Digital Dust Library` folder under `Лор`, `Char`, and related
  material — **not** in this repo.

Whether the two identities are ever publicly linked is an open decision (see below), not
a structural one — keeping them on separate domains/repos preserves the option either
way. **Default: do not mix Ryan Kobary / Glitch fiction material into this repo.**

## What lives in this repo vs. the parent folder

This repo (`DigitalDustLibrary`) = blog only:
- `Content_Ecosystem_Structure.md` — the source-of-truth doc for how all the pieces
  (LinkedIn, blog, Glitch, Ryan Kobary, haykbaroyan.com) fit together.

Stays in the parent `Digital Dust Library` folder, not this repo (Glitch/Ryan Kobary
material and unrelated assets):
- `Лор` — Glitch lore/world-building docs.
- `Char` — character reference images for Glitch.
- `Logo`, `Glitch_web` — Glitch branding/site assets.
- `games.txt`, `headings.txt`, `structure.txt`, `Фразы-Глитча.txt` — Glitch video
  production notes (background game footage, video format lengths, script phrases).

If new content is added to the parent folder, ask which side of the identity split it
belongs to before deciding whether it should be copied into this repo.

## Tech stack

Decided (2026-07), versions current as of decision time — bump as needed, don't treat
these as pinned forever:

**Backend**
- .NET 10 (LTS) — ASP.NET Core Web API
- EF Core 10
- PostgreSQL 18.x
- ASP.NET Core Identity + JWT bearer auth (decoupled SPA frontends, not cookie/BFF)
- Resend for transactional email (author-application notifications) — free tier is
  3,000 emails/month, permanent, not a trial. Revisit only if marketing/newsletter
  email is ever needed (Brevo would be the pick there — handles both transactional
  and marketing, free tier ~9,000/month).

**Blog frontend (public-facing, `apps/blog`)**
- Svelte 5 (runes) + SvelteKit 2.x
- TypeScript 6.0
- Vite 8 (Rolldown-powered bundler under the hood as of Vite 8 — no usage change)
- Tailwind CSS 4.3.x + `@tailwindcss/typography` for article prose rendering
- Deploys to Cloudflare (Pages/Workers via `@sveltejs/adapter-cloudflare`), prerendered
  (SSG) per-route where content doesn't change often
- SEO: per-page meta/OG tags via `<svelte:head>`, JSON-LD structured data
  (Article/BlogPosting schema), sitemap + RSS as SvelteKit server routes (RSS feed
  also intended to feed the Glitch video-script pipeline)

**Admin frontend (authoring/moderation, `apps/admin`)**
- React 19.2.x
- Vite 8 + TypeScript 6.0
- shadcn/ui (Radix-based, copy-paste component ownership — pairs with Tailwind)
- TipTap for the rich-text/markdown post editor — open-source core is MIT-licensed
  and free; only TipTap's Cloud Platform (real-time co-editing, comments, AI) is
  paid, and isn't needed since editing is single-author-at-a-time, not simultaneous.

**Monorepo**
- Turborepo 2.10.x
- Layout: `apps/blog`, `apps/admin`, `apps/api`, plus `packages/shared-types`
  (TS types generated from the .NET API's OpenAPI spec, e.g. via NSwag or
  openapi-typescript — keeps frontend types in sync with backend DTOs automatically),
  `packages/shared-utils`, `packages/validation` (e.g. Zod/Valibot schemas shared
  between admin and blog where both need the same rules)
- Blog and admin are two fully separate deployed apps (not a microfrontend/composed
  runtime) — the monorepo is purely a filing/shared-package convenience
- Node.js 24 (current LTS line; Node 26 becomes LTS Oct 2026, revisit then)

**State management**: no external library — Svelte 5 runes cover the blog's needs
natively; React admin state is light enough (auth session, post-review UI, form
state) not to need Redux/Zustand at this scale.

## Multi-author platform (planned, not yet MVP-required)

Originally a single-author blog; the plan is to open it up to other contributors
under the same Digital Dust Library brand — a shared magazine/group-blog model
(everyone publishes under `digitaldustlibrary.com` with author bylines), not
separate per-author sites.

- **Roles**: Owner (Hayk, full control incl. approving authors/posts), Editor
  (can review/approve others' posts), Author (can draft/submit own posts only,
  can't touch others').
- **Post status workflow**: `Draft` → `Pending Review` → `Published`, with a
  `Changes Requested` state for sending feedback back to the author instead of a
  flat reject. A lightweight review-notes table (post_id, reviewer_id, comment,
  created_at) carries that feedback.
- **Becoming an author**: public application flow (a form: who they are, what they
  want to write about) stored in a separate `AuthorApplications` table — NOT the
  `Users` table, since applicants aren't real accounts until approved. Owner/Editor
  reviews and approves or rejects; approval creates a real `User` with the Author
  role and triggers a Resend email so they can set a password and log in.
- Public application endpoint needs basic abuse protection (rate limiting at
  minimum) since it's unauthenticated.
- `Posts` needs an `AuthorId` FK from day one even while Hayk is the only author —
  avoids a painful migration later.

## Conventions

- Article drafts are Word docs (`.docx`) inside their own subfolder per topic.
- Office lock/temp files (`~$*.docx`, `~WRL*.tmp`) are gitignored — never commit these.
- No fixed naming scheme enforced yet beyond "one folder per article topic" — adopt this
  loosely until/unless it becomes a problem.

## Hosting / deployment

Split hosting, a deliberate exception to the "everything on one droplet" pattern
used by other projects:

- **Blog (`apps/blog`, SvelteKit)**: deploys to Cloudflare (Pages/Workers via
  `adapter-cloudflare`). Public-facing, benefits from edge rendering and Cloudflare's
  perf/SEO characteristics.
- **API (`apps/api`, .NET) + PostgreSQL + admin (`apps/admin`, React)**: stay on the
  existing DigitalOcean droplet (see the `haykbaroyan-hosting` project memory for
  droplet/Caddy/Docker Compose conventions), same Caddy-site-block-per-project
  pattern as `haykbaroyan.com` and other projects on that droplet.
- Cloudflare still in front for DNS on all domains regardless of which piece is
  edge-hosted vs droplet-hosted.
- Practical implication: the blog calls the API over the public internet (not same-
  host/same-Caddy-block), so CORS needs to be configured on the API for the blog's
  Cloudflare domain.
- Not yet deployed as of this writing — domain is reserved, site isn't live.

## Open decisions (deliberately deferred, don't resolve unilaterally)

- Whether Ryan Kobary is ever mentioned on the Hayk Baroyan CV/site.
- Whether the Glitch YouTube channel publicly attributes itself to Ryan Kobary.
- Final domain for the Ryan Kobary author site (availability check still needed).
- Whether `ryankobary.com` shares the droplet with the other sites or gets hosted
  separately — shared infra is a minor fingerprinting/leak risk given the deliberate
  identity separation, worth weighing if separation matters a lot.

## GitHub account note

Repo is intended to live under a single consolidated GitHub account (in the process of
merging `hayk0505` and `hayk-baroyan` into one — see chat history / commit author
config for current state). Don't be surprised if commit authorship looks inconsistent
across early commits while that's being sorted out.
