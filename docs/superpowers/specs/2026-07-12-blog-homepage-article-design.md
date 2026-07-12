# Blog Homepage + Article Detail Page (design)

Date: 2026-07-12

## Purpose

Build the first real UI in `apps/blog`, matching two design mockups (`Blog Home.pdf`,
`Blog item.pdf`): the homepage (3-pillar column grid with featured/teaser cards) and the
article detail page (long-form reading view with drop cap). The scaffold pass
(`2026-07-11-monorepo-scaffold-design.md`) deliberately left `apps/blog` as bare
SvelteKit output with no styling library and no content — this pass installs Tailwind,
builds the component library, and wires up mock content so both designed pages render.

There is no `apps/api` yet, so this pass uses a local mock data module as the content
source, structured so it's a drop-in swap for real API calls later.

## Scope

In scope:
- Homepage (`/`): 3-pillar column grid, matching `Blog Home.pdf`.
- Article detail page (`/articles/[slug]`): matching `Blog item.pdf`.
- Page stubs (so nav links don't 404): archive listing, author profile, become-an-author.
- Tailwind CSS 4 + typography plugin installation.
- Mock data layer for posts/authors/pillars.

Explicitly out of scope (future passes):
- Real API integration (`apps/api` doesn't exist yet).
- Become-an-author form submission logic (no backend to submit to).
- Admin panel (`apps/admin`) — separate app, separate spec.
- Auth / login flow — the blog's "Log in" nav link points at the (future) admin app;
  no auth is implemented in the blog itself.
- Search, pagination/infinite scroll on the archive page.
- Real images (featured images stay placeholder boxes).

## Architecture

### Routing

```
src/lib/
  config.ts                             # site-wide constants, incl. ADMIN_URL for "Log in"

src/routes/
  +layout.svelte                        # minimal shell: fonts, global CSS, <slot/> — no header
  +error.svelte                         # small on-brand error page
  (site)/+layout.svelte                 # SiteHeader + <slot/> + footer
  (site)/+page.svelte                   # homepage
  (site)/archive/+page.svelte           # full listing (reuses PostTeaserRow)
  (site)/authors/[handle]/+page.svelte  # author profile stub
  (site)/become-an-author/+page.svelte  # stub page, no form submission logic
  articles/[slug]/+page.svelte          # article detail — own ArticleTopBar
  articles/[slug]/+page.ts              # load(): getPostBySlug(slug), error(404) if missing
```

`(site)` is a SvelteKit route group (doesn't affect the URL): it wraps
homepage/archive/author/become-an-author with the full nav header. The article detail
route sits outside that group because its top bar (back-link + wordmark + share, no nav)
is visually distinct from the rest of the site — this avoids threading a header-variant
prop through one shared layout.

### Data layer (`src/lib/data/`)

```ts
type Pillar = { slug: string; label: string; index: number; accent: 'red' | 'green' | 'blue' };
type Author = { handle: string; name: string; role?: string; avatarColor: string };
type Post = {
  slug: string;
  title: string;
  excerpt: string;
  pillarSlug: string;
  authorHandle: string;
  publishedAt: string;      // ISO date string
  readingMinutes: number;
  dispatchNumber: number;   // decorative "04 / 07 / 12" shown on featured cards
  featured: boolean;
  body: string;             // HTML for the article detail page
};
```

- `pillars.ts` — the 3 canonical pillars (Tech, Social·Psych, Software Dev), each with a
  slug, display label, order index, and accent color key.
- `authors.ts` — mock author records matching the mockup's bylines (Sam Okafor, Maren
  Osei, etc.), each with a declared `avatarColor` (no hash-based derivation — simpler and
  avoids "clever" logic for a cosmetic value).
- `posts.ts` — 12 mock posts (4 per pillar), including `dispatchNumber` and `featured`
  as declared fields, not computed — matches the mockup directly without needing
  cross-pillar numbering logic that would have to stay in sync for zero real benefit.
- `index.ts` — query functions consumed by route `load()` functions:
  `getAllPosts()`, `getPostsByPillar(pillarSlug)`, `getFeaturedPostForPillar(pillarSlug)`,
  `getPostBySlug(slug)`, `getRelatedPosts(post, limit)`, `getPostsByAuthor(handle)`.
  This is the seam that gets replaced with real API calls once `apps/api` exists — route
  `load()` functions only ever call these functions, never touch the raw arrays.

### Components (`src/lib/components/`)

**`layout/`**
- `SiteHeader.svelte` — wordmark, tagline, nav (Archive / Become an Author / Log in).
  "Log in" links to the (future) admin app URL, read from `ADMIN_URL` in
  `src/lib/config.ts`, not a blog-side auth route.
- `ArticleTopBar.svelte` — back-to-columns link, wordmark, share controls. Used only by
  the article detail route.

**`home/`**
- `IssueBanner.svelte` — "VOL. 04 — JULY 2026" / "3 pillars · N dispatches" strip.
  Dispatch count is derived from `getAllPosts().length`, not hardcoded twice.
- `PillarColumn.svelte` — one column: `PillarBadge` + `FeaturedPostCard` + list of
  `PostTeaserRow` + "— END OF COLUMN —" footer. Props: a `Pillar` and its `Post[]`.
- `PillarBadge.svelte` — colored dot + "PILLAR 0X" + post count + pillar title + underline.
- `FeaturedPostCard.svelte` — dark hero card variant (top of each column): dispatch
  number, title, excerpt, `AuthorByline`.
- `PostTeaserRow.svelte` — compact row variant: `PillarTag`-style date/time header,
  title, excerpt, `AuthorByline`.

**`article/`**
- `ArticleMeta.svelte` — eyebrow (pillar tag / date / reading time), title, dek,
  divider, author row.
- `FeaturedImage.svelte` — placeholder box (diagonal stripe pattern + dimension label);
  swappable for a real `<img>` once real images exist.
- `ArticleBody.svelte` — renders the post's HTML body with `@tailwindcss/typography`'s
  `prose` class; first-letter drop cap via CSS `::first-letter`, no manual markup split.
- `ShareLinks.svelte` — share affordance + LinkedIn link.

**`shared/`**
- `Avatar.svelte` — initials circle, colored via the author's declared `avatarColor`.
- `AuthorByline.svelte` — `Avatar` + name (+ optional role, + optional reading time slot).
- `PillarTag.svelte` — colored dot + pillar label; shared between the article eyebrow and
  each teaser row's header.

### Utils (`src/lib/utils/`)

- `initials.ts` — `getInitials(name: string): string` (e.g. "Sam Okafor" → "SO").
- `format-date.ts` — `formatDispatchDate(iso: string): string` (e.g. → "JUL 07"), used by
  both teaser rows and the article eyebrow so date formatting only lives in one place.

## Styling

- Install `tailwindcss` + `@tailwindcss/vite` (Vite-native integration, no PostCSS config
  file needed) and `@tailwindcss/typography`.
- Design tokens (pillar accent colors, font families) defined once via Tailwind v4's
  `@theme` block in a global `app.css`, imported from the root `+layout.svelte`.
- Fonts via `@fontsource` packages (bundled at build time, not a runtime CDN request —
  matters for the Cloudflare-edge/SEO goals in `CLAUDE.md`):
  - **Fraunces** (serif) — headlines, drop caps, decorative dispatch numerals.
  - **IBM Plex Mono** — all-caps letter-spaced labels/meta ("PILLAR 01", "FEATURED",
    dates, "SHARE"). These are an initial pick, centralized in the `@theme` tokens so
    they're a one-line change later if the brand direction shifts.
- Utility classes written directly in components (no `@apply` abstraction layer) — the
  component breakdown above already keeps each piece small, so utility classes stay
  readable without needing an extra indirection.

## Error handling

- `articles/[slug]/+page.ts` calls `error(404, 'Post not found')` for an unknown slug,
  rendered by a small on-brand `+error.svelte` instead of SvelteKit's default blank page.
- `become-an-author` stub renders static content only — no form submission wiring, since
  there's no backend endpoint yet to submit to (building that now would be throwaway
  code once the real API/spec for that endpoint exists).

## Verification

No test runner is installed in `apps/blog` (explicitly skipped in the scaffold pass —
nothing existed to test yet). For this pass:
- `pnpm --filter blog check` (svelte-check) and `pnpm --filter blog build` both succeed.
- `pnpm --filter blog lint` passes.
- Manual verification via the dev server: load the homepage and confirm all three
  columns render with correct featured/teaser split; click through to an article detail
  page and confirm the drop cap, author row, and back link work; visit the archive,
  author-profile, and become-an-author stub routes to confirm they render instead of
  404ing; visit an unknown slug and confirm the branded error page renders.

## Out of scope / open questions carried forward

- Real API wiring — becomes its own spec once `apps/api` exists; the `data/index.ts`
  query functions are the intended seam for that swap.
- Become-an-author form fields/submission — needs its own design pass once there's a
  backend endpoint to hit (see `Functional_Overview_for_Design.md` for the intended
  fields: name, email, pitch).
- Archive page filtering/pagination beyond a flat chronological list.
- Real featured images, OG image generation, and the RSS/sitemap routes mentioned in
  `CLAUDE.md`'s SEO plan.
