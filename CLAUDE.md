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
- **Local dev orchestration**: `docker-compose.dev.yml` at the repo root (API +
  Postgres), not a `package.json` wrapper around `dotnet watch`. Turborepo stays
  scoped to the JS/TS apps (blog, admin); Docker Compose handles the
  cross-language piece, mirroring the production shape (droplet, Compose, Caddy)
  instead of stretching Turborepo to cover a language it doesn't understand.
- **`apps/api` status (2026-08-06)**: running and verified end-to-end against a
  real Postgres instance via `docker-compose.dev.yml` (not just written —
  actually built, migrated, and exercised with real requests). Working
  endpoints: **auth** (login/refresh/logout/accept-invite, in-memory-JWT +
  httpOnly-refresh-cookie model), **posts** (list/create, matches
  `apps/admin`'s Phase 1 MSW contract field-for-field; `PATCH` is now
  author-only — own posts only, and can only self-move between `Draft`/
  `PendingReview`, except Editor/Owner authors may also self-move straight
  to `Published` (setting `PublishedAt`), skipping `Pending Review` — they
  already have approve authority over everyone else's posts, so reviewing
  their own first is redundant; `ChangesRequested` stays blocked via `PATCH`
  for every role, since an author can't meaningfully request changes from
  themselves; `POST /{id}/approve` and `POST /{id}/request-changes`
  (both Editor/Owner) are the only way to reach `Published`/
  `ChangesRequested` for an Author's own posts — approve sets `PublishedAt`, request-changes requires
  a non-empty comment and creates a `ReviewNote`. `PostDto` now includes
  `AuthorName` (populated via `.Include(p => p.Author)` on the relevant
  queries — `POST /` create is the one exception, so a just-created post's
  `authorName` in its own response is empty), which `apps/admin`'s new
  Review Queue/Review Detail screens rely on, since `GET /api/users` is
  Owner-only and Editors need to see author names too), **media**
  (list/create), **categories** (list/create/patch/delete — hide via
  `PATCH isVisible`, soft-delete/restore via `PATCH isDeleted`, hard `DELETE`
  blocked with a 409 when any post still references the category),
  **author applications** (public rate-limited submit — 5/hour/IP — plus
  Editor/Owner list/approve/reject; approve creates a real `User`
  (`IsActive: false`) and an `InviteToken`, emails an invite link, and
  `POST /api/auth/accept-invite` redeems it — sets the real password,
  activates the account, and logs the user in), **users & roles**
  (Owner-only list/change-role/deactivate — self-lockout guarded, so an
  Owner can't demote or deactivate their own account; deactivating revokes
  the user's active refresh tokens immediately rather than only blocking
  their next login, and `POST /api/auth/refresh` independently re-checks
  the user's `IsActive` as a backstop). `Migrations/InitialCreate` and
  `Migrations/AddInviteTokens` exist and have been applied. Three demo
  accounts seeded on startup (`author@dd.local`, `editor@dd.local`,
  `owner@dd.local`, password `password` for all — see
  `apps/api/Data/DbSeeder.cs`).
  Automated test coverage exists: `apps/api.Tests` (xUnit +
  `WebApplicationFactory` + Testcontainers.PostgreSql, real Postgres per test
  run, no mocked DB) — 74 tests covering categories, applications, the
  invite/accept flow, users & roles, and the post review/publish flow end
  to end.
  Email (Resend): wired via `IEmailSender` — `LoggingEmailSender` is what dev
  and every test actually exercise (no `Resend:ApiKey` configured anywhere
  yet); `ResendEmailSender` exists but hasn't been verified against a real
  Resend account/API key.
  **EF Core gotcha found while building request-changes**: adding a new
  child entity *only* via a navigation collection (e.g.
  `post.ReviewNotes.Add(new ReviewNote { ... })`) rather than explicitly to
  its own `DbSet` (`db.ReviewNotes.Add(...)`) throws a spurious
  `DbUpdateConcurrencyException` ("0 rows affected") here — EF misreads the
  client-generated `Guid` key as an existing row to `UPDATE` rather than a
  new one to `INSERT` when the entity is only discovered via graph fixup.
  Every entity in this codebase is added straight to its `DbSet` for exactly
  this reason; keep doing that for any new child-entity inserts.
  **`dotnet watch` hot-reload gotcha (found 2026-08-06, cost real debugging
  time)**: the `docker-compose.dev.yml` API container's `dotnet watch`
  process crashed once early in a long dev session (a known bug in its
  file-polling watcher on Docker bind mounts — `PollingDirectoryWatcher`
  throwing `An item with the same key has already been added`), auto-
  restarted, and every code change after that point applied via hot reload
  (`🔥 C# and Razor changes applied`) instead of a real process restart. Hot
  reload patches existing method *bodies* in place but does not re-run
  startup-time route registration (`app.MapPostEndpoints()` and friends) —
  so a brand-new route added after that point (e.g. `/approve`,
  `/request-changes`) silently never made it into the running server's
  route table, returning a bare framework 404 with no JSON body (not the
  endpoint's own `{"message": "Not found"}`), even though the source was
  correct and `dotnet test` passed (tests boot their own fresh process each
  time via `WebApplicationFactory`, so they never see this). **Fix: `docker
  restart digitaldustlibrary-api-1` whenever a newly-added route 404s with
  an empty body despite existing in source** — don't trust hot reload for
  anything that adds a new endpoint, only for body-logic edits to routes
  that already existed when the container last fully started.
  Swagger UI's "Authorize" button is wired up (`Program.cs`,
  `AddSwaggerGen`) — the earlier note about Swashbuckle v10's
  security-scheme API being unsettled is resolved: v10.2.3 removed
  `OpenApiSecurityScheme.Reference`, so `AddSecurityRequirement` now takes
  `Func<OpenApiDocument, OpenApiSecurityRequirement>` with the scheme
  referenced via `new OpenApiSecuritySchemeReference("bearer", document)`
  instead of the old inline-`Reference` idiom. Applies the Bearer
  requirement globally (every endpoint shows a lock icon in the UI,
  including public ones) — cosmetic only, actual authorization is still
  each endpoint's own `RequireAuthorization()`/`AllowAnonymous()`.
  Still schema-only, no endpoints: **Statistics**. Also
  still open: no global exception-handling middleware yet (errors currently
  rely on each endpoint's own `Results.Json(..., statusCode: ...)` calls).
  The admin Categories screen (`apps/admin/src/pages/Categories.tsx`) is now
  built against the existing `CategoryEndpoints.cs` contract — list/create/
  visibility-toggle/archive/restore/hard-delete all have a UI.
  **Media uploads (2026-08-10)**: real disk storage — `POST /api/media`
  decodes the client's data URL, writes it to `wwwroot/uploads/{guid}.{ext}`,
  and stores an absolute static URL (not the data: URI) on
  `MediaAsset.Url` — absolute rather than relative since admin and the API
  may sit on different origins in production. Images only (png/jpeg/webp/
  gif/svg+xml), 8 MB cap, both enforced server-side. Static files are served
  unauthenticated (`app.UseStaticFiles()` in `Program.cs`, scoped to
  `/uploads` via an explicit `PhysicalFileProvider`) since the public blog
  also renders these via `PublicPostDto.FeaturedImageUrl`. **Gotcha found
  while building this**: `wwwroot` didn't exist anywhere in this repo before
  now — ASP.NET Core freezes `IWebHostEnvironment.WebRootFileProvider` to a
  `NullFileProvider` for the app's entire lifetime if `wwwroot` doesn't exist
  at startup, so `UseStaticFiles()`'s default (relying on that env-provided
  file provider) would silently 404 every uploaded file forever, even after
  the folder gets created later at runtime. Fixed by using an explicit
  `PhysicalFileProvider` scoped to the uploads path, with the directory
  created eagerly before `UseStaticFiles()` is called — this also means any
  future static-file need in this API should do the same (explicit
  `PhysicalFileProvider`, not the bare `env.WebRootFileProvider` default)
  rather than assuming `wwwroot` exists. The admin Media Library
  (`apps/admin/src/pages/MediaLibrary.tsx`) now has a real file picker
  (hidden `<input type="file">`, `FileReader` + `Image.onload` for real
  dimensions) — previously "Upload new" just re-uploaded a hardcoded
  placeholder SVG rectangle every click, with no file selection UI at all.

**Blog frontend (public-facing, `apps/blog`)**
- Svelte 5 (runes) + SvelteKit 2.x
- TypeScript 6.0
- Vite 8 (Rolldown-powered bundler under the hood as of Vite 8 — no usage change)
- Tailwind CSS 4.3.x + `@tailwindcss/typography` for article prose rendering
- Deploys to Cloudflare Workers via `@sveltejs/adapter-cloudflare`. Fully
  server-rendered per-request against the live API (`GET /api/public/*`),
  not prerendered/SSG — posts are DB-backed and change independently of
  deploys, so there's nothing static to pre-generate at build time.
- SEO: per-page meta/OG tags via `<svelte:head>`, JSON-LD structured data
  (Article/BlogPosting schema), sitemap + RSS as SvelteKit server routes (RSS feed
  also intended to feed the Glitch video-script pipeline)
- **Status (2026-08-10)**: wired end-to-end against the real API (no more
  static mock data) — homepage, archive, article, and author pages all fetch
  live data via `src/lib/api.ts`. RSS (`/rss.xml`) and sitemap (`/sitemap.xml`)
  are real SvelteKit `+server.ts` routes. `adapter-cloudflare` + `wrangler.jsonc`
  are in place and verified locally via `wrangler dev` (homepage, an article
  route, RSS, and sitemap all confirmed serving real rendered content from a
  built `.svelte-kit/cloudflare/_worker.js`) — **not yet actually deployed**,
  no `wrangler login`/`wrangler deploy` has been run against a real Cloudflare
  account. `apps/blog/.env.production` holds a placeholder `PUBLIC_API_URL`
  (`https://api.digitaldustlibrary.com/api/public` — this domain doesn't
  resolve yet) since the var is inlined at build time via `$env/static/public`;
  replace it with the real API origin once one exists. Two gotchas found
  while verifying: (1) the Wrangler version installed at the time
  (4.120.0)'s bundled `workerd` runtime capped `compatibility_date` two days
  behind the actual date — if `wrangler dev`/`wrangler deploy` ever reports
  a date-not-supported error, lower `wrangler.jsonc`'s `compatibility_date`
  to whatever the error says is the newest supported value. (2) Cloudflare's
  `workerd` runtime enforces real CORS on outbound `fetch()` calls made
  *from inside* a Worker during SSR — unlike Node/`adapter-node`, where
  server-side fetches are never CORS-checked. This means the eventual live
  deploy needs the blog's real Cloudflare domain in the API's CORS
  allowlist for SSR page loads to work at all, not just for client-side
  calls — this isn't a local-dev-only quirk, it'll matter in production too.

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

## Working conventions with Claude

- Do not run `git add` / `git commit` on this repo unless explicitly asked in the
  moment. The user reviews and commits changes manually. This applies even when a
  task is otherwise complete and ready to commit — finish the work and stop, don't
  commit it.

## GitHub account note

Repo is intended to live under a single consolidated GitHub account (in the process of
merging `hayk0505` and `hayk-baroyan` into one — see chat history / commit author
config for current state). Don't be surprised if commit authorship looks inconsistent
across early commits while that's being sorted out.
