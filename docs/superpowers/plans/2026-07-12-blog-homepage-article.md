# Blog Homepage + Article Detail Page Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the first real UI in `apps/blog` — a 3-pillar homepage grid and a
long-form article detail page — matching `Blog Home.pdf` and `Blog item.pdf`, backed by
a local mock data layer designed as a drop-in seam for a future real API.

**Architecture:** SvelteKit route group `(site)` wraps the shared-header pages
(homepage, archive, author profile, become-an-author); the article detail route sits
outside that group with its own minimal top bar. A typed `src/lib/data` module (pillars,
authors, posts + query functions) is the only thing route `load()` functions touch — no
route ever imports the raw arrays directly. Tailwind CSS 4 (installed fresh this pass)
provides styling via utility classes and a small `@theme` token block for the 3 pillar
accent colors and two font families.

**Tech Stack:** SvelteKit 2 (Svelte 5 runes), TypeScript, Tailwind CSS 4
(`@tailwindcss/vite`) + `@tailwindcss/typography`, `@fontsource/fraunces` +
`@fontsource/ibm-plex-mono`.

## Global Constraints

- Tailwind CSS 4 via `tailwindcss` + `@tailwindcss/vite` — no `tailwind.config.js`, no
  PostCSS config file; theme customization lives in a CSS `@theme` block.
- `@tailwindcss/typography` registered via CSS `@plugin '@tailwindcss/typography';`
  (v4 CSS-first plugin syntax), used for the article body's `prose` class.
- Fonts ship via `@fontsource/fraunces` and `@fontsource/ibm-plex-mono` (bundled at
  build time) — no Google Fonts CDN `<link>`.
- No new test runner: Vitest/Playwright stay uninstalled (per the prior scaffold pass).
  Verification is `svelte-check` + `build` + `lint` + a manual dev-server walkthrough
  against the two PDFs.
- No `@apply` abstraction layer — Tailwind utility classes go directly in components.
- Tailwind class names must always be full literal strings, never built via template
  interpolation (e.g. `` `bg-accent-${accent}` ``) — v4's scanner only picks up literal
  class text in source, so dynamic construction silently renders unstyled. Any
  per-variant class must come from a static lookup object (see `pillarAccentClasses`
  in Task 2).
- The `(site)` route group wraps homepage/archive/author-profile/become-an-author with
  `SiteHeader`; `articles/[slug]` stays outside it and renders its own `ArticleTopBar`.
- Route `load()` functions only ever call `src/lib/data` query functions
  (`getAllPosts`, `getPostBySlug`, etc.) — never import the raw `posts`/`authors`
  arrays directly from a route file.
- `become-an-author` renders static content only — no form submission wiring, since
  there's no backend endpoint yet to submit to.
- Prettier config (`apps/blog/prettier.config.js`): tabs, single quotes, no trailing
  commas, printWidth 100. Run `pnpm --filter blog format` after adding new files in a
  task before committing.
- All cross-file imports from outside `src/lib/data/` go through the `src/lib/data`
  barrel (`$lib/data`), never a deep import like `$lib/data/pillars` — one public
  surface for the data layer, matching its role as the future API-swap seam.

---

### Task 1: Install Tailwind CSS 4 + fonts, wire global styles

**Files:**
- Modify: `apps/blog/package.json` (new dependencies)
- Modify: `apps/blog/vite.config.ts`
- Create: `apps/blog/src/app.css`
- Modify: `apps/blog/src/routes/+layout.svelte`

**Interfaces:**
- Produces: global Tailwind utilities plus custom theme utilities `bg-accent-red`,
  `bg-accent-green`, `bg-accent-blue` (and their `text-*`/`border-*` equivalents),
  `bg-paper`, `text-ink` (and opacity variants like `text-ink/60`), `font-display`
  (Fraunces), `font-label` (IBM Plex Mono), and the `prose`/`prose-neutral` classes
  from `@tailwindcss/typography`. All later tasks depend on these class names existing.

- [ ] **Step 1: Install Tailwind and font packages**

Run:
```bash
pnpm --filter blog add -D tailwindcss @tailwindcss/vite @tailwindcss/typography
pnpm --filter blog add @fontsource/fraunces @fontsource/ibm-plex-mono
```
Expected: both commands exit 0 and add entries to `apps/blog/package.json`.

- [ ] **Step 2: Register the Tailwind Vite plugin**

Replace the full contents of `apps/blog/vite.config.ts` with:

```ts
import adapter from '@sveltejs/adapter-auto';
import { sveltekit } from '@sveltejs/kit/vite';
import tailwindcss from '@tailwindcss/vite';
import { defineConfig } from 'vite';

export default defineConfig({
	plugins: [
		tailwindcss(),
		sveltekit({
			compilerOptions: {
				// Force runes mode for the project, except for libraries. Can be removed in svelte 6.
				runes: ({ filename }) =>
					filename.split(/[/\\]/).includes('node_modules') ? undefined : true
			},

			// adapter-auto only supports some environments, see https://svelte.dev/docs/kit/adapter-auto for a list.
			// If your environment is not supported, or you settled on a specific environment, switch out the adapter.
			// See https://svelte.dev/docs/kit/adapters for more information about adapters.
			adapter: adapter()
		})
	]
});
```

- [ ] **Step 3: Create the global stylesheet with theme tokens**

Create `apps/blog/src/app.css`:

```css
@import 'tailwindcss';
@plugin '@tailwindcss/typography';

@import '@fontsource/fraunces/400.css';
@import '@fontsource/fraunces/400-italic.css';
@import '@fontsource/fraunces/700.css';
@import '@fontsource/ibm-plex-mono/400.css';
@import '@fontsource/ibm-plex-mono/500.css';

@theme {
	--color-accent-red: #dc2626;
	--color-accent-green: #059669;
	--color-accent-blue: #2563eb;
	--color-ink: #1c1a17;
	--color-paper: #f4f1ea;
	--font-display: 'Fraunces', ui-serif, serif;
	--font-label: 'IBM Plex Mono', ui-monospace, monospace;
}

body {
	background-color: var(--color-paper);
	color: var(--color-ink);
	font-family: var(--font-display);
}
```

- [ ] **Step 4: Import the stylesheet from the root layout**

Replace the full contents of `apps/blog/src/routes/+layout.svelte` with:

```svelte
<script lang="ts">
	import '../app.css';

	let { children } = $props();
</script>

<svelte:head>
	<link rel="icon" href={favicon} />
</svelte:head>

{@render children()}
```

- [ ] **Step 5: Visually confirm Tailwind is active**

Temporarily edit `apps/blog/src/routes/+page.svelte` (it will be fully replaced in
Task 8, so this edit is throwaway) to:

```svelte
<h1 class="font-display text-4xl font-bold text-accent-blue">Welcome to SvelteKit</h1>
<p>Visit <a href="https://svelte.dev/docs/kit">svelte.dev/docs/kit</a> to read the documentation</p>
```

Run: `pnpm --filter blog dev`

Open the printed local URL (typically `http://localhost:5173`) and confirm the heading
renders in a large serif font, colored blue, on a cream (`#f4f1ea`) page background.
Stop the dev server once confirmed.

- [ ] **Step 6: Type-check and commit**

Run: `pnpm --filter blog check`
Expected: no errors.

```bash
git add apps/blog/package.json apps/blog/pnpm-lock.yaml apps/blog/vite.config.ts apps/blog/src/app.css apps/blog/src/routes/+layout.svelte apps/blog/src/routes/+page.svelte
git commit -m "Install Tailwind CSS 4 and configure design tokens/fonts"
```

---

### Task 2: Data types, pillars, and authors

**Files:**
- Create: `apps/blog/src/lib/data/types.ts`
- Create: `apps/blog/src/lib/data/pillars.ts`
- Create: `apps/blog/src/lib/data/authors.ts`

**Interfaces:**
- Consumes: nothing from earlier tasks.
- Produces: types `PillarAccent`, `Pillar`, `Author`, `Post`; `pillars: Pillar[]`;
  `pillarAccentClasses: Record<PillarAccent, { dot: string; text: string; border: string; bg: string }>`;
  `getPillarBySlug(slug: string): Pillar | undefined`; `authors: Author[]`;
  `getAuthorByHandle(handle: string): Author | undefined`. All of Task 3 onward import
  these only via the `$lib/data` barrel created in Task 3.

- [ ] **Step 1: Write the shared types**

Create `apps/blog/src/lib/data/types.ts`:

```ts
export type PillarAccent = 'red' | 'green' | 'blue';

export type Pillar = {
	slug: string;
	label: string;
	index: number;
	accent: PillarAccent;
};

export type Author = {
	handle: string;
	name: string;
	role: string;
	avatarColor: string;
};

export type Post = {
	slug: string;
	title: string;
	excerpt: string;
	pillarSlug: string;
	authorHandle: string;
	publishedAt: string;
	readingMinutes: number;
	dispatchNumber: number;
	featured: boolean;
	body: string;
};
```

- [ ] **Step 2: Write the pillar registry and accent class lookup**

Create `apps/blog/src/lib/data/pillars.ts`:

```ts
import type { Pillar, PillarAccent } from './types';

export const pillars: Pillar[] = [
	{ slug: 'tech', label: 'Tech', index: 1, accent: 'red' },
	{ slug: 'social-psych', label: 'Social · Psych', index: 2, accent: 'green' },
	{ slug: 'software-dev', label: 'Software Dev', index: 3, accent: 'blue' }
];

type AccentClasses = { dot: string; text: string; border: string; bg: string };

// Full literal class strings only — Tailwind's scanner can't see dynamically
// interpolated class names, so each variant is spelled out here rather than built
// from a template string.
export const pillarAccentClasses: Record<PillarAccent, AccentClasses> = {
	red: {
		dot: 'bg-accent-red',
		text: 'text-accent-red',
		border: 'border-accent-red',
		bg: 'bg-accent-red'
	},
	green: {
		dot: 'bg-accent-green',
		text: 'text-accent-green',
		border: 'border-accent-green',
		bg: 'bg-accent-green'
	},
	blue: {
		dot: 'bg-accent-blue',
		text: 'text-accent-blue',
		border: 'border-accent-blue',
		bg: 'bg-accent-blue'
	}
};

export function getPillarBySlug(slug: string): Pillar | undefined {
	return pillars.find((pillar) => pillar.slug === slug);
}
```

- [ ] **Step 3: Write the mock author roster**

Create `apps/blog/src/lib/data/authors.ts`:

```ts
import type { Author } from './types';

export const authors: Author[] = [
	{ handle: 'theo-vance', name: 'Theo Vance', role: 'Tech Desk', avatarColor: 'bg-amber-500' },
	{ handle: 'maren-osei', name: 'Maren Osei', role: 'Tech Desk', avatarColor: 'bg-rose-500' },
	{ handle: 'priya-anand', name: 'Priya Anand', role: 'Tech Desk', avatarColor: 'bg-emerald-500' },
	{ handle: 'ada-reyes', name: 'Ada Reyes', role: 'Culture Desk', avatarColor: 'bg-violet-500' },
	{ handle: 'jonah-pike', name: 'Jonah Pike', role: 'Culture Desk', avatarColor: 'bg-sky-500' },
	{ handle: 'lena-hart', name: 'Lena Hart', role: 'Culture Desk', avatarColor: 'bg-fuchsia-500' },
	{ handle: 'sam-okafor', name: 'Sam Okafor', role: 'Engineering Desk', avatarColor: 'bg-blue-600' },
	{ handle: 'iris-wong', name: 'Iris Wong', role: 'Engineering Desk', avatarColor: 'bg-teal-500' }
];

export function getAuthorByHandle(handle: string): Author | undefined {
	return authors.find((author) => author.handle === handle);
}
```

- [ ] **Step 4: Type-check and format**

Run:
```bash
pnpm --filter blog format
pnpm --filter blog check
```
Expected: format rewrites nothing unexpected (or applies quote/indent normalization),
check passes with no errors. There is no visual result yet — these modules aren't
imported anywhere until Task 3.

- [ ] **Step 5: Commit**

```bash
git add apps/blog/src/lib/data/types.ts apps/blog/src/lib/data/pillars.ts apps/blog/src/lib/data/authors.ts
git commit -m "Add pillar and author mock data with accent class lookup"
```

---

### Task 3: Mock posts and the data-layer query barrel

**Files:**
- Create: `apps/blog/src/lib/data/posts.ts`
- Create: `apps/blog/src/lib/data/index.ts`

**Interfaces:**
- Consumes: `Post`, `Pillar`, `Author`, `PillarAccent` types and `pillars`/`authors`
  from Task 2 (re-exported, not re-declared).
- Produces (all via the `$lib/data` barrel): `getAllPosts(): Post[]`,
  `getPostsByPillar(pillarSlug: string): Post[]`,
  `getFeaturedPostForPillar(pillarSlug: string): Post | undefined`,
  `getNonFeaturedPostsForPillar(pillarSlug: string): Post[]`,
  `getPostBySlug(slug: string): Post | undefined`,
  `getRelatedPosts(post: Post, limit?: number): Post[]`,
  `getPostsByAuthor(handle: string): Post[]`, plus re-exports of `pillars`,
  `pillarAccentClasses`, `getPillarBySlug`, `authors`, `getAuthorByHandle`, and all
  types from Task 2. Every later task imports from `$lib/data`, never from
  `$lib/data/posts` or `$lib/data/pillars` directly.

- [ ] **Step 1: Write the 12 mock posts**

Create `apps/blog/src/lib/data/posts.ts`:

```ts
import type { Post } from './types';

export const posts: Post[] = [
	{
		slug: 'reading-the-rings-of-a-data-center',
		title: 'Reading the Rings of a Data Center',
		excerpt:
			'Racks age in visible layers. You can date a facility by its cabling the way you date a tree by its rings.',
		pillarSlug: 'tech',
		authorHandle: 'priya-anand',
		publishedAt: '2026-06-15',
		readingMinutes: 7,
		dispatchNumber: 1,
		featured: false,
		body: `<p>Racks age in visible layers. You can date a facility by its cabling the way you date a tree by its rings — the color of the patch cables, the generation of switch gear, the dust pattern behind a cooling unit that's been running since a decommissioned product launch.</p>
<p>Walk any old data hall long enough and you can point at a row and say: this is when we still believed in that acquisition. The hardware doesn't lie about what mattered when it was installed, even after everyone who installed it has moved on.</p>`
	},
	{
		slug: 'cold-storage-and-the-myth-of-permanence',
		title: 'Cold Storage and the Myth of Permanence',
		excerpt:
			'Tape survives longer than the company that wrote to it. A tour of the places we send data to be forgotten slowly.',
		pillarSlug: 'tech',
		authorHandle: 'maren-osei',
		publishedAt: '2026-06-24',
		readingMinutes: 9,
		dispatchNumber: 2,
		featured: false,
		body: `<p>Tape survives longer than the company that wrote to it. Walk into any long-term archival facility and you'll find drives spinning down decades after the businesses that filled them went quiet.</p>
<p>There's something almost comic about it: the format outlives the reason it was chosen. LTO tape was picked for cost, not sentiment, and yet it's the closest thing the industry has to permanence — not because it's built to last forever, but because nobody's finished migrating off it yet.</p>
<p>Permanence, it turns out, is mostly a migration schedule nobody's gotten around to running.</p>`
	},
	{
		slug: 'what-the-internet-forgets-on-purpose',
		title: 'What the Internet Forgets on Purpose',
		excerpt: 'Deletion is rarely an accident. Someone, somewhere, decided this was not worth the storage bill.',
		pillarSlug: 'tech',
		authorHandle: 'theo-vance',
		publishedAt: '2026-07-02',
		readingMinutes: 8,
		dispatchNumber: 3,
		featured: false,
		body: `<p>Deletion is rarely an accident. Someone, somewhere, decided this was not worth the storage bill, and a form that used to exist stopped existing, quietly, without a farewell tour.</p>
<p>We like to talk about the internet forgetting things as if forgetting were a failure of the system. Mostly it's the opposite: a decision, made by someone with a budget, about what counted as worth keeping. The blob storage bill doesn't care about your nostalgia.</p>
<p>What's left after a purge like that tells you more about a company's priorities than its mission statement ever could. Look at what survived the last cost-cutting pass, and you've found what they actually valued.</p>`
	},
	{
		slug: 'half-life-of-a-hyperlink',
		title: 'The Half-Life of a Hyperlink',
		excerpt:
			'Every link you post is quietly counting down. A field guide to link rot, and why the average URL outlives a housefly by only a few years.',
		pillarSlug: 'tech',
		authorHandle: 'maren-osei',
		publishedAt: '2026-07-04',
		readingMinutes: 11,
		dispatchNumber: 4,
		featured: true,
		body: `<p>Every link you post is quietly counting down from the moment you hit publish. Domains lapse, companies fold, content management systems get replaced by other content management systems, and the URL that felt permanent turns out to have been rented, not owned.</p>
<p>Researchers who have tried to measure this call it link rot, and the numbers are worse than most people expect: a meaningful fraction of links in any given web page are dead within a few years, and the average shared URL outlives a housefly by only a handful of summers. The web remembers less than we think it does, and it forgets faster than we're built to notice.</p>
<p>The fix isn't heroic — it's boring, which is why almost nobody does it. Archive what you link to. Prefer permalinks with stable identifiers over query strings that break the moment a site redesigns. Treat every outbound link in something you've written as a small promise you probably won't be able to keep.</p>`
	},
	{
		slug: 'digital-grief-and-the-ghosts-in-our-inboxes',
		title: 'Digital Grief and the Ghosts in Our Inboxes',
		excerpt: 'The dead keep sending calendar invites. On mourning people whose accounts outlive them.',
		pillarSlug: 'social-psych',
		authorHandle: 'lena-hart',
		publishedAt: '2026-06-11',
		readingMinutes: 10,
		dispatchNumber: 5,
		featured: false,
		body: `<p>The dead keep sending calendar invites. Birthday reminders fire on schedule, autoplay suggests a video from an account that hasn't posted in three years, and a subscription renews for a service its owner will never log into again.</p>
<p>We built systems that assume everyone using them is still alive, and now we're stuck maintaining etiquette for a situation none of the interfaces were designed to hold.</p>`
	},
	{
		slug: 'why-we-hoard-screenshots-we-never-reopen',
		title: 'Why We Hoard Screenshots We Never Reopen',
		excerpt:
			'Ten thousand images in the camera roll, opened once. On saving as a substitute for remembering.',
		pillarSlug: 'social-psych',
		authorHandle: 'ada-reyes',
		publishedAt: '2026-06-21',
		readingMinutes: 6,
		dispatchNumber: 6,
		featured: false,
		body: `<p>Ten thousand images in the camera roll, opened once. On saving as a substitute for remembering — the screenshot as a promise to your future self that you'll come back to this, a promise you both know you won't keep.</p>
<p>The saving itself is the relief. Once it's captured, the brain quietly reclassifies the moment as handled, filed, no longer its job to hold onto. The archive grows; the memory doesn't.</p>`
	},
	{
		slug: 'the-loneliness-of-the-long-scrolling-user',
		title: 'The Loneliness of the Long-Scrolling User',
		excerpt: 'Infinite feeds promised company and delivered a very crowded kind of solitude.',
		pillarSlug: 'social-psych',
		authorHandle: 'jonah-pike',
		publishedAt: '2026-06-30',
		readingMinutes: 9,
		dispatchNumber: 8,
		featured: false,
		body: `<p>Infinite feeds promised company and delivered a very crowded kind of solitude — a room full of people, none of whom know you're there.</p>
<p>The design is not an accident. Engagement is easier to sell than connection, and a feed that never ends is a feed that never has to justify why you're still watching.</p>`
	},
	{
		slug: 'nostalgia-as-a-compression-algorithm',
		title: 'Nostalgia as a Compression Algorithm',
		excerpt:
			'Memory does not store the past; it stores a lossy summary and reconstructs the rest. What the internet does to us, we already did to ourselves.',
		pillarSlug: 'social-psych',
		authorHandle: 'ada-reyes',
		publishedAt: '2026-07-03',
		readingMinutes: 12,
		dispatchNumber: 7,
		featured: true,
		body: `<p>Memory does not store the past; it stores a lossy summary and reconstructs the rest on demand, filling gaps with whatever fits the shape of the story you already believe about yourself.</p>
<p>This isn't a flaw so much as an engineering tradeoff. A brain that stored every frame at full fidelity would run out of room by adolescence. So it keeps the compressed version — the highlight reel, the emotional average — and discards the raw footage.</p>
<p>What the internet does to us, with its algorithmic feeds resurfacing old photos on cue, we already did to ourselves first. It just automated the part we used to do quietly, alone, at 2 a.m.</p>`
	},
	{
		slug: 'comments-are-messages-to-a-stranger',
		title: 'Comments Are Messages to a Stranger',
		excerpt:
			'You write them for a future maintainer you will never meet. Usually that maintainer is you, and you are furious.',
		pillarSlug: 'software-dev',
		authorHandle: 'iris-wong',
		publishedAt: '2026-06-09',
		readingMinutes: 6,
		dispatchNumber: 9,
		featured: false,
		body: `<p>You write them for a future maintainer you will never meet. Usually that maintainer is you, and you are furious — furious that past-you didn't explain the workaround, furious that the comment you're about to write will be read by someone equally unprepared.</p>
<p>The best comments aren't documentation. They're apologies, written in advance, for a decision that made sense under a deadline and won't make sense to anyone reading it cold.</p>`
	},
	{
		slug: 'deprecation-is-a-kind-of-mourning',
		title: 'Deprecation Is a Kind of Mourning',
		excerpt:
			"Sunsetting an API means telling everyone who depended on it that their world is ending — politely, in a changelog.",
		pillarSlug: 'software-dev',
		authorHandle: 'iris-wong',
		publishedAt: '2026-06-19',
		readingMinutes: 7,
		dispatchNumber: 10,
		featured: false,
		body: `<p>Sunsetting an API means telling everyone who depended on it that their world is ending — politely, in a changelog, with a migration guide nobody reads until the day it matters.</p>
<p>There's a particular kind of guilt in writing a deprecation notice. You're not just removing a function; you're informing strangers, some of whom built entire businesses on your promise that this would keep working, that the promise had a shelf life you never advertised.</p>`
	},
	{
		slug: 'the-archaeology-of-a-node-modules-folder',
		title: 'The Archaeology of a node_modules Folder',
		excerpt:
			'Dig deep enough and you find abandoned packages, dead maintainers, and forks of forks of forks.',
		pillarSlug: 'software-dev',
		authorHandle: 'sam-okafor',
		publishedAt: '2026-06-28',
		readingMinutes: 8,
		dispatchNumber: 11,
		featured: false,
		body: `<p>Dig deep enough and you find abandoned packages, dead maintainers, and forks of forks of forks — a dependency tree is a family tree, and like most family trees, nobody agrees on who's actually still speaking to whom.</p>
<p>Every node_modules folder is a small museum of decisions nobody remembers making: a polyfill for a browser nobody supports anymore, a utility library pulled in for one function that could've been six lines.</p>`
	},
	{
		slug: 'legacy-code-is-a-love-letter',
		title: 'Legacy Code Is a Love Letter',
		excerpt:
			'The ugliest function in the codebase is usually the one that kept the company alive. A defense of the code nobody wants to touch.',
		pillarSlug: 'software-dev',
		authorHandle: 'sam-okafor',
		publishedAt: '2026-07-07',
		readingMinutes: 10,
		dispatchNumber: 12,
		featured: true,
		body: `<p>For a long time we told ourselves that the web was permanent — that once a thing was posted it was posted forever, indexed and immortal. The truth is closer to the opposite. What we publish begins decaying the moment it lands, and most of it is gone within a decade, quietly, without ceremony or obituary.</p>
<p>The mechanisms of this decay are dull. A company folds. A subdomain lapses. A migration goes half-finished and the old URLs stop resolving. No villain, no fire — just entropy doing the unglamorous work it always does, one dead link at a time.</p>
<p>What interests me is not the loss itself but what the loss reveals about how we valued the thing in the first place. We keep what we are paid to keep, and we lose almost everything else. The archive is not a record of what mattered; it is a record of what was profitable to remember.</p>
<p>There is a version of this story that is purely mournful, and I have told it that way before. But there is another reading, and lately I prefer it: forgetting is not a bug in the system — it's a feature of how anything alive stays legible. A codebase that remembered every dead branch, every abandoned experiment, every function nobody had the heart to delete, would not be a codebase. It would be a museum with the lights left on in every room at once.</p>
<p>That is what legacy code actually is, underneath the jokes about spaghetti and the groaning in code review. It is a record of every deadline the company survived by choosing done over clean. The ugliest function in the repository is rarely a mistake — it is a scar, and scars are proof that something kept living long enough to get one.</p>`
	}
];
```

- [ ] **Step 2: Write the query barrel**

Create `apps/blog/src/lib/data/index.ts`:

```ts
import { posts } from './posts';
import type { Post } from './types';

export * from './types';
export { authors, getAuthorByHandle } from './authors';
export { getPillarBySlug, pillarAccentClasses, pillars } from './pillars';

export function getAllPosts(): Post[] {
	return [...posts].sort((a, b) => (a.publishedAt < b.publishedAt ? 1 : -1));
}

export function getPostsByPillar(pillarSlug: string): Post[] {
	return getAllPosts().filter((post) => post.pillarSlug === pillarSlug);
}

export function getFeaturedPostForPillar(pillarSlug: string): Post | undefined {
	return posts.find((post) => post.pillarSlug === pillarSlug && post.featured);
}

export function getNonFeaturedPostsForPillar(pillarSlug: string): Post[] {
	return getPostsByPillar(pillarSlug).filter((post) => !post.featured);
}

export function getPostBySlug(slug: string): Post | undefined {
	return posts.find((post) => post.slug === slug);
}

export function getRelatedPosts(post: Post, limit = 3): Post[] {
	return getPostsByPillar(post.pillarSlug)
		.filter((candidate) => candidate.slug !== post.slug)
		.slice(0, limit);
}

export function getPostsByAuthor(handle: string): Post[] {
	return getAllPosts().filter((post) => post.authorHandle === handle);
}
```

- [ ] **Step 3: Type-check and format**

Run:
```bash
pnpm --filter blog format
pnpm --filter blog check
```
Expected: no errors. (Behavioral proof that these functions return the right data comes
in Task 8 and Task 10, once a route actually renders their output — there's no unit
test runner in this project to exercise them standalone.)

- [ ] **Step 4: Commit**

```bash
git add apps/blog/src/lib/data/posts.ts apps/blog/src/lib/data/index.ts
git commit -m "Add mock posts and the data-layer query barrel"
```

---

### Task 4: Formatting utils

**Files:**
- Create: `apps/blog/src/lib/utils/initials.ts`
- Create: `apps/blog/src/lib/utils/format-date.ts`

**Interfaces:**
- Consumes: nothing.
- Produces: `getInitials(name: string): string`, `formatDispatchDate(iso: string): string`.
  Both are consumed starting in Task 5.

- [ ] **Step 1: Write `getInitials`**

Create `apps/blog/src/lib/utils/initials.ts`:

```ts
export function getInitials(name: string): string {
	return name
		.split(' ')
		.filter(Boolean)
		.map((part) => part[0]?.toUpperCase() ?? '')
		.join('')
		.slice(0, 2);
}
```

- [ ] **Step 2: Write `formatDispatchDate`**

Create `apps/blog/src/lib/utils/format-date.ts`:

```ts
export function formatDispatchDate(iso: string): string {
	// timeZone: 'UTC' avoids an off-by-one day for users behind UTC, since publish
	// dates are stored as bare YYYY-MM-DD with no time component.
	return new Date(iso)
		.toLocaleDateString('en-US', { month: 'short', day: '2-digit', timeZone: 'UTC' })
		.toUpperCase();
}
```

- [ ] **Step 3: Type-check**

Run: `pnpm --filter blog check`
Expected: no errors. (Visual proof these produce the right strings comes once
`PostTeaserRow`/`ArticleMeta`/`Avatar` render them on screen, in Tasks 6/8/9/10.)

- [ ] **Step 4: Commit**

```bash
git add apps/blog/src/lib/utils/initials.ts apps/blog/src/lib/utils/format-date.ts
git commit -m "Add initials and dispatch-date formatting utils"
```

---

### Task 5: Site config and shared components (Avatar, AuthorByline, PillarDot)

**Files:**
- Create: `apps/blog/src/lib/config.ts`
- Create: `apps/blog/src/lib/components/shared/Avatar.svelte`
- Create: `apps/blog/src/lib/components/shared/AuthorByline.svelte`
- Create: `apps/blog/src/lib/components/shared/PillarDot.svelte`

**Interfaces:**
- Consumes: `getInitials` (Task 4), `pillarAccentClasses`/`PillarAccent`/`Author`
  (Task 2/3, via `$lib/data`).
- Produces: `ADMIN_URL: string`; `Avatar` (props: `name: string`, `colorClass: string`,
  `size?: 'sm' | 'md'`); `AuthorByline` (props: `author: Author`, `meta?: string`);
  `PillarDot` (props: `accent: PillarAccent`, `size?: 'sm' | 'md'`). All three
  components are used starting in Task 6.

- [ ] **Step 1: Write the site config constant**

Create `apps/blog/src/lib/config.ts`:

```ts
// Points at the separate apps/admin app once it's deployed — the blog itself has no
// auth of its own (see CLAUDE.md hosting split).
export const ADMIN_URL = 'https://admin.digitaldustlibrary.com';
```

- [ ] **Step 2: Write `PillarDot`**

Create `apps/blog/src/lib/components/shared/PillarDot.svelte`:

```svelte
<script lang="ts">
	import { pillarAccentClasses, type PillarAccent } from '$lib/data';

	let { accent, size = 'sm' }: { accent: PillarAccent; size?: 'sm' | 'md' } = $props();

	const sizeClass = size === 'md' ? 'h-2.5 w-2.5' : 'h-1.5 w-1.5';
</script>

<span class="inline-block rounded-full {pillarAccentClasses[accent].dot} {sizeClass}"></span>
```

- [ ] **Step 3: Write `Avatar`**

Create `apps/blog/src/lib/components/shared/Avatar.svelte`:

```svelte
<script lang="ts">
	import { getInitials } from '$lib/utils/initials';

	let {
		name,
		colorClass,
		size = 'md'
	}: { name: string; colorClass: string; size?: 'sm' | 'md' } = $props();

	const sizeClasses = size === 'sm' ? 'h-6 w-6 text-[10px]' : 'h-8 w-8 text-xs';
</script>

<span
	class="inline-flex items-center justify-center rounded-full font-label font-medium text-white {colorClass} {sizeClasses}"
>
	{getInitials(name)}
</span>
```

- [ ] **Step 4: Write `AuthorByline`**

Create `apps/blog/src/lib/components/shared/AuthorByline.svelte`:

```svelte
<script lang="ts">
	import type { Author } from '$lib/data';
	import Avatar from './Avatar.svelte';

	let { author, meta }: { author: Author; meta?: string } = $props();
</script>

<div class="flex items-center gap-2">
	<Avatar name={author.name} colorClass={author.avatarColor} />
	<div class="leading-tight">
		<p class="text-sm font-semibold">{author.name}</p>
		{#if meta}
			<p class="font-label text-[11px] tracking-wide text-ink/60 uppercase">{meta}</p>
		{/if}
	</div>
</div>
```

- [ ] **Step 5: Type-check, format, commit**

Run:
```bash
pnpm --filter blog format
pnpm --filter blog check
```
Expected: no errors. No visual check yet — nothing imports these components until
Task 6.

```bash
git add apps/blog/src/lib/config.ts apps/blog/src/lib/components/shared
git commit -m "Add ADMIN_URL config and shared Avatar/AuthorByline/PillarDot components"
```

---

### Task 6: Homepage components

**Files:**
- Create: `apps/blog/src/lib/components/home/PillarBadge.svelte`
- Create: `apps/blog/src/lib/components/home/FeaturedPostCard.svelte`
- Create: `apps/blog/src/lib/components/home/PostTeaserRow.svelte`
- Create: `apps/blog/src/lib/components/home/PillarColumn.svelte`
- Create: `apps/blog/src/lib/components/home/IssueBanner.svelte`

**Interfaces:**
- Consumes: `AuthorByline`, `PillarDot` (Task 5); `pillarAccentClasses`,
  `getAuthorByHandle`, `Pillar`, `Post`, `Author`, `PillarAccent` (Task 2/3, via
  `$lib/data`); `formatDispatchDate` (Task 4).
- Produces: `PillarColumn` (props: `pillar: Pillar`, `posts: Post[]`) and
  `IssueBanner` (props: `volumeLabel: string`, `dispatchCount: number`) — these two are
  what Task 8's homepage route consumes directly; the other three are internal to this
  folder.

- [ ] **Step 1: Write `PillarBadge`**

Create `apps/blog/src/lib/components/home/PillarBadge.svelte`:

```svelte
<script lang="ts">
	import { pillarAccentClasses, type Pillar } from '$lib/data';
	import PillarDot from '../shared/PillarDot.svelte';

	let { pillar, postCount }: { pillar: Pillar; postCount: number } = $props();
</script>

<header class="mb-4">
	<div class="flex items-center justify-between font-label text-xs tracking-widest uppercase">
		<span class="flex items-center gap-2">
			<PillarDot accent={pillar.accent} size="md" />
			Pillar 0{pillar.index}
		</span>
		<span class="text-ink/50">{postCount} posts</span>
	</div>
	<h2 class="mt-1 font-display text-2xl font-bold uppercase">{pillar.label}</h2>
	<div class="mt-2 h-1 w-10 {pillarAccentClasses[pillar.accent].bg}"></div>
</header>
```

- [ ] **Step 2: Write `FeaturedPostCard`**

Create `apps/blog/src/lib/components/home/FeaturedPostCard.svelte`:

```svelte
<script lang="ts">
	import { pillarAccentClasses, type Author, type PillarAccent, type Post } from '$lib/data';
	import AuthorByline from '../shared/AuthorByline.svelte';

	let { post, author, accent }: { post: Post; author: Author; accent: PillarAccent } = $props();
</script>

<a
	href="/articles/{post.slug}"
	class="relative block overflow-hidden rounded-md bg-ink p-6 text-paper"
>
	<span class="font-label text-xs tracking-widest text-paper/60 uppercase">Featured</span>
	<span
		class="pointer-events-none absolute top-2 right-4 font-display text-7xl font-bold {pillarAccentClasses[
			accent
		].text}"
	>
		{String(post.dispatchNumber).padStart(2, '0')}
	</span>
	<h3 class="mt-2 max-w-[80%] font-display text-xl font-bold">{post.title}</h3>
	<p class="mt-2 text-sm text-paper/80">{post.excerpt}</p>
	<div class="mt-4 flex items-center justify-between border-t border-paper/20 pt-4">
		<AuthorByline {author} />
		<span class="font-label text-xs text-paper/60">{post.readingMinutes} min</span>
	</div>
</a>
```

- [ ] **Step 3: Write `PostTeaserRow`**

Create `apps/blog/src/lib/components/home/PostTeaserRow.svelte`:

```svelte
<script lang="ts">
	import type { Author, PillarAccent, Post } from '$lib/data';
	import { formatDispatchDate } from '$lib/utils/format-date';
	import AuthorByline from '../shared/AuthorByline.svelte';
	import PillarDot from '../shared/PillarDot.svelte';

	let { post, author, accent }: { post: Post; author: Author; accent: PillarAccent } = $props();
</script>

<a href="/articles/{post.slug}" class="block border-t border-ink/10 py-4 first:border-t-0">
	<div class="flex items-center gap-2 font-label text-xs tracking-widest text-ink/60 uppercase">
		<PillarDot {accent} />
		{formatDispatchDate(post.publishedAt)}
		<span class="text-ink/30">·</span>
		{post.readingMinutes} min
	</div>
	<h3 class="mt-1 font-display text-lg font-bold">{post.title}</h3>
	<p class="mt-1 text-sm text-ink/70">{post.excerpt}</p>
	<div class="mt-3">
		<AuthorByline {author} />
	</div>
</a>
```

- [ ] **Step 4: Write `PillarColumn`**

Create `apps/blog/src/lib/components/home/PillarColumn.svelte`:

```svelte
<script lang="ts">
	import { getAuthorByHandle, type Pillar, type Post } from '$lib/data';
	import FeaturedPostCard from './FeaturedPostCard.svelte';
	import PillarBadge from './PillarBadge.svelte';
	import PostTeaserRow from './PostTeaserRow.svelte';

	let { pillar, posts }: { pillar: Pillar; posts: Post[] } = $props();

	const featuredPost = posts.find((post) => post.featured);
	const restPosts = posts.filter((post) => !post.featured);
</script>

<section>
	<PillarBadge {pillar} postCount={posts.length} />

	{#if featuredPost}
		{@const featuredAuthor = getAuthorByHandle(featuredPost.authorHandle)}
		{#if featuredAuthor}
			<FeaturedPostCard post={featuredPost} author={featuredAuthor} accent={pillar.accent} />
		{/if}
	{/if}

	{#each restPosts as post (post.slug)}
		{@const author = getAuthorByHandle(post.authorHandle)}
		{#if author}
			<PostTeaserRow {post} {author} accent={pillar.accent} />
		{/if}
	{/each}

	<p class="mt-4 text-center font-label text-xs tracking-widest text-ink/40 uppercase">
		— End of column —
	</p>
</section>
```

- [ ] **Step 5: Write `IssueBanner`**

Create `apps/blog/src/lib/components/home/IssueBanner.svelte`:

```svelte
<script lang="ts">
	let { volumeLabel, dispatchCount }: { volumeLabel: string; dispatchCount: number } = $props();
</script>

<div
	class="flex flex-col gap-1 border-b border-ink/10 pb-3 font-label text-xs tracking-widest text-ink/60 uppercase sm:flex-row sm:items-center sm:justify-between"
>
	<span>{volumeLabel}</span>
	<span>Three pillars · {dispatchCount} dispatches · Scroll each column ↓</span>
</div>
```

- [ ] **Step 6: Type-check, format, commit**

Run:
```bash
pnpm --filter blog format
pnpm --filter blog check
```
Expected: no errors. No visual check yet — these aren't wired into a route until
Task 8.

```bash
git add apps/blog/src/lib/components/home
git commit -m "Add homepage column, featured card, and teaser row components"
```

---

### Task 7: Site header layout and branded error page

**Files:**
- Create: `apps/blog/src/lib/components/layout/SiteHeader.svelte`
- Create: `apps/blog/src/routes/(site)/+layout.svelte`
- Create: `apps/blog/src/routes/+error.svelte`

**Interfaces:**
- Consumes: `ADMIN_URL` (Task 5).
- Produces: the `(site)` route group layout that Tasks 8 and 11's routes live inside.

- [ ] **Step 1: Write `SiteHeader`**

Create `apps/blog/src/lib/components/layout/SiteHeader.svelte`:

```svelte
<script lang="ts">
	import { ADMIN_URL } from '$lib/config';
</script>

<header class="border-b border-ink/10 bg-paper">
	<div class="mx-auto flex max-w-6xl items-center justify-between px-6 py-4">
		<a href="/" class="leading-tight">
			<span class="font-label text-lg font-bold tracking-tight uppercase">
				Digital Dust <span class="text-accent-red">Library</span>
			</span>
			<p class="font-label text-[10px] tracking-widest text-ink/50 uppercase">
				Field notes on what the internet leaves behind
			</p>
		</a>
		<nav class="flex items-center gap-6 font-label text-xs tracking-widest uppercase">
			<a href="/archive" class="hover:text-accent-red">Archive</a>
			<a href="/become-an-author" class="hover:text-accent-red">Become an author</a>
			<a href={ADMIN_URL} class="rounded border border-ink px-3 py-1.5 hover:bg-ink hover:text-paper">
				Log in
			</a>
		</nav>
	</div>
</header>
```

- [ ] **Step 2: Create the `(site)` route group layout**

Create `apps/blog/src/routes/(site)/+layout.svelte`:

```svelte
<script lang="ts">
	import SiteHeader from '$lib/components/layout/SiteHeader.svelte';

	let { children } = $props();
</script>

<SiteHeader />

<main class="mx-auto max-w-6xl px-6 py-8">
	{@render children()}
</main>
```

- [ ] **Step 3: Move the homepage placeholder into the route group**

The scaffold's `+page.svelte` currently lives at `apps/blog/src/routes/+page.svelte`.
Move it into the group so it picks up the new layout:

```bash
mkdir -p "apps/blog/src/routes/(site)"
git mv apps/blog/src/routes/+page.svelte "apps/blog/src/routes/(site)/+page.svelte"
```

(This keeps the throwaway Tailwind test content from Task 1 for now — Task 8 replaces
the file's contents entirely.)

- [ ] **Step 4: Create the branded error page**

Create `apps/blog/src/routes/+error.svelte`:

```svelte
<script lang="ts">
	import { page } from '$app/state';
</script>

<div class="flex min-h-screen flex-col items-center justify-center gap-3 bg-paper px-6 text-center">
	<p class="font-label text-xs tracking-widest text-ink/50 uppercase">Error {page.status}</p>
	<h1 class="font-display text-3xl font-bold">
		{page.error?.message ?? 'Something went missing'}
	</h1>
	<a href="/" class="font-label text-xs tracking-widest text-accent-red uppercase hover:underline">
		← Back to all columns
	</a>
</div>
```

- [ ] **Step 5: Visually verify header and error page**

Run: `pnpm --filter blog dev`

Open the printed local URL and confirm:
- The homepage now shows the `SiteHeader` (wordmark, tagline, Archive / Become an
  author / Log in nav) above the still-placeholder Tailwind test content.
- Visiting a nonexistent path (e.g. append `/does-not-exist` to the URL) renders the
  branded error page (not SvelteKit's default blank error screen), showing "Error 404"
  and a "Not Found" message with a link back to `/`.

Stop the dev server once confirmed.

- [ ] **Step 6: Type-check and commit**

Run: `pnpm --filter blog check`
Expected: no errors.

```bash
git add apps/blog/src/lib/components/layout apps/blog/src/routes
git commit -m "Add SiteHeader, (site) route group layout, and branded error page"
```

---

### Task 8: Homepage route

**Files:**
- Create: `apps/blog/src/routes/(site)/+page.ts`
- Modify: `apps/blog/src/routes/(site)/+page.svelte`

**Interfaces:**
- Consumes: `pillars`, `getPostsByPillar`, `getAllPosts` (Task 3, via `$lib/data`);
  `IssueBanner`, `PillarColumn` (Task 6).
- Produces: the `/` route, fully matching `Blog Home.pdf`.

- [ ] **Step 1: Write the homepage load function**

Create `apps/blog/src/routes/(site)/+page.ts`:

```ts
import { getPostsByPillar, pillars } from '$lib/data';
import type { PageLoad } from './$types';

export const load: PageLoad = () => {
	const columns = pillars.map((pillar) => ({
		pillar,
		posts: getPostsByPillar(pillar.slug)
	}));

	return { columns };
};
```

- [ ] **Step 2: Replace the homepage template**

Replace the full contents of `apps/blog/src/routes/(site)/+page.svelte` with:

```svelte
<script lang="ts">
	import { getAllPosts } from '$lib/data';
	import IssueBanner from '$lib/components/home/IssueBanner.svelte';
	import PillarColumn from '$lib/components/home/PillarColumn.svelte';
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();
</script>

<svelte:head>
	<title>Digital Dust Library — Field notes on what the internet leaves behind</title>
</svelte:head>

<IssueBanner volumeLabel="Vol. 04 — July 2026" dispatchCount={getAllPosts().length} />

<div class="mt-8 grid gap-10 md:grid-cols-3">
	{#each data.columns as column (column.pillar.slug)}
		<PillarColumn pillar={column.pillar} posts={column.posts} />
	{/each}
</div>
```

- [ ] **Step 3: Full visual verification against `Blog Home.pdf`**

Run: `pnpm --filter blog build` first to confirm the production build succeeds, then
`pnpm --filter blog dev` and open the printed local URL.

Confirm, side-by-side with `Blog Home.pdf`:
- Issue banner reads "VOL. 04 — JULY 2026" and "THREE PILLARS · 12 DISPATCHES · SCROLL
  EACH COLUMN ↓".
- Three columns render: Tech (red accent), Social · Psych (green accent), Software Dev
  (blue accent), each showing "4 POSTS".
- Each column's featured card is a dark card with the correct large colored dispatch
  number (04 / 07 / 12 respectively), title, excerpt, author byline, and reading time.
- Each column lists its remaining 3 posts as teaser rows with date, reading time,
  title, excerpt, and author byline, in reverse-chronological order.
- Each column ends with "— End of column —".

Stop the dev server once confirmed.

- [ ] **Step 4: Type-check, lint, and commit**

Run:
```bash
pnpm --filter blog format
pnpm --filter blog check
pnpm --filter blog lint
```
Expected: no errors.

```bash
git add apps/blog/src/routes/(site)/+page.ts apps/blog/src/routes/(site)/+page.svelte
git commit -m "Wire up the homepage 3-pillar grid"
```

---

### Task 9: Article detail components

**Files:**
- Create: `apps/blog/src/lib/components/article/ShareLinks.svelte`
- Create: `apps/blog/src/lib/components/article/ArticleTopBar.svelte`
- Create: `apps/blog/src/lib/components/article/ArticleMeta.svelte`
- Create: `apps/blog/src/lib/components/article/FeaturedImage.svelte`
- Create: `apps/blog/src/lib/components/article/ArticleBody.svelte`

**Interfaces:**
- Consumes: `pillars`, `pillarAccentClasses`, `Post`, `Author` (Task 2/3, via
  `$lib/data`); `PillarDot`, `AuthorByline` (Task 5); `formatDispatchDate` (Task 4).
- Produces: `ArticleTopBar` (props: `shareUrl: string`),
  `ArticleMeta` (props: `post: Post`, `author: Author`), `FeaturedImage` (props:
  `width?: number`, `height?: number`), `ArticleBody` (props: `html: string`) — all
  consumed by Task 10's article route.

- [ ] **Step 1: Write `ShareLinks`**

Create `apps/blog/src/lib/components/article/ShareLinks.svelte`:

```svelte
<script lang="ts">
	let { url }: { url: string } = $props();

	const linkedInHref = `https://www.linkedin.com/sharing/share-offsite/?url=${encodeURIComponent(url)}`;
</script>

<div class="flex items-center gap-3 font-label text-xs tracking-widest uppercase">
	<span class="text-ink/50">Share</span>
	<a
		href={linkedInHref}
		target="_blank"
		rel="noopener noreferrer"
		class="rounded border border-ink/20 px-2 py-1 hover:border-ink"
	>
		in
	</a>
</div>
```

- [ ] **Step 2: Write `ArticleTopBar`**

Create `apps/blog/src/lib/components/article/ArticleTopBar.svelte`:

```svelte
<script lang="ts">
	import ShareLinks from './ShareLinks.svelte';

	let { shareUrl }: { shareUrl: string } = $props();
</script>

<div class="border-b border-ink/10 bg-paper">
	<div class="mx-auto flex max-w-3xl items-center justify-between px-6 py-4">
		<a href="/" class="font-label text-xs tracking-widest text-ink/70 uppercase hover:text-ink">
			← All columns
		</a>
		<a href="/" class="font-label text-sm font-bold tracking-tight uppercase">
			Digital Dust <span class="text-accent-red">Library</span>
		</a>
		<ShareLinks url={shareUrl} />
	</div>
</div>
```

- [ ] **Step 3: Write `ArticleMeta`**

Create `apps/blog/src/lib/components/article/ArticleMeta.svelte`:

```svelte
<script lang="ts">
	import { pillarAccentClasses, pillars, type Author, type Post } from '$lib/data';
	import { formatDispatchDate } from '$lib/utils/format-date';
	import AuthorByline from '$lib/components/shared/AuthorByline.svelte';
	import PillarDot from '$lib/components/shared/PillarDot.svelte';

	let { post, author }: { post: Post; author: Author } = $props();

	const pillar = pillars.find((candidate) => candidate.slug === post.pillarSlug);
</script>

{#if pillar}
	<div class="flex items-center gap-2 font-label text-xs tracking-widest uppercase">
		<PillarDot accent={pillar.accent} size="md" />
		<span class={pillarAccentClasses[pillar.accent].text}>{pillar.label}</span>
		<span class="text-ink/30">/</span>
		<span class="text-ink/60">{formatDispatchDate(post.publishedAt)}</span>
		<span class="text-ink/30">/</span>
		<span class="text-ink/60">{post.readingMinutes} min</span>
	</div>
{/if}

<h1 class="mt-3 font-display text-4xl font-bold">{post.title}</h1>
<p class="mt-3 font-display text-lg text-ink/70 italic">{post.excerpt}</p>

<div class="mt-6 border-t border-ink/10 pt-6">
	<AuthorByline {author} meta={author.role} />
</div>
```

- [ ] **Step 4: Write `FeaturedImage`**

Create `apps/blog/src/lib/components/article/FeaturedImage.svelte`:

```svelte
<script lang="ts">
	let { width = 1600, height = 900 }: { width?: number; height?: number } = $props();
</script>

<div
	class="flex aspect-video w-full items-center justify-center rounded-md border border-dashed border-ink/20"
	style="background-image: repeating-linear-gradient(135deg, rgba(28, 26, 23, 0.06) 0 10px, transparent 10px 20px);"
>
	<span class="font-label text-xs tracking-widest text-ink/40 uppercase">
		Featured image — {width}×{height}
	</span>
</div>
```

- [ ] **Step 5: Write `ArticleBody`**

Create `apps/blog/src/lib/components/article/ArticleBody.svelte`:

```svelte
<script lang="ts">
	// html always comes from our own mock data, never user input, so @html is safe here.
	let { html }: { html: string } = $props();
</script>

<div class="article-body prose prose-neutral mt-8 max-w-none">
	{@html html}
</div>

<style>
	.article-body :global(p:first-of-type::first-letter) {
		float: left;
		margin-right: 0.5rem;
		font-family: var(--font-display);
		font-size: 4.5rem;
		font-weight: 700;
		line-height: 0.85;
		color: var(--color-accent-blue);
	}
</style>
```

- [ ] **Step 6: Type-check, format, commit**

Run:
```bash
pnpm --filter blog format
pnpm --filter blog check
```
Expected: no errors. No visual check yet — these aren't wired into a route until
Task 10.

```bash
git add apps/blog/src/lib/components/article
git commit -m "Add article detail components: top bar, meta, image, body"
```

---

### Task 10: Article detail route

**Files:**
- Create: `apps/blog/src/routes/articles/[slug]/+page.ts`
- Create: `apps/blog/src/routes/articles/[slug]/+page.svelte`

**Interfaces:**
- Consumes: `getPostBySlug`, `getAuthorByHandle` (Task 3, via `$lib/data`);
  `ArticleTopBar`, `ArticleMeta`, `FeaturedImage`, `ArticleBody` (Task 9).
- Produces: the `/articles/[slug]` route, fully matching `Blog item.pdf`, plus a 404
  for unknown slugs handled by Task 7's `+error.svelte`.

- [ ] **Step 1: Write the article load function**

Create `apps/blog/src/routes/articles/[slug]/+page.ts`:

```ts
import { getAuthorByHandle, getPostBySlug } from '$lib/data';
import { error } from '@sveltejs/kit';
import type { PageLoad } from './$types';

export const load: PageLoad = ({ params }) => {
	const post = getPostBySlug(params.slug);

	if (!post) {
		error(404, 'Post not found');
	}

	const author = getAuthorByHandle(post.authorHandle);

	if (!author) {
		error(404, 'Author not found');
	}

	return { post, author };
};
```

- [ ] **Step 2: Write the article page template**

Create `apps/blog/src/routes/articles/[slug]/+page.svelte`:

```svelte
<script lang="ts">
	import ArticleBody from '$lib/components/article/ArticleBody.svelte';
	import ArticleMeta from '$lib/components/article/ArticleMeta.svelte';
	import ArticleTopBar from '$lib/components/article/ArticleTopBar.svelte';
	import FeaturedImage from '$lib/components/article/FeaturedImage.svelte';
	import { page } from '$app/state';
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();
</script>

<svelte:head>
	<title>{data.post.title} — Digital Dust Library</title>
	<meta name="description" content={data.post.excerpt} />
</svelte:head>

<ArticleTopBar shareUrl={page.url.href} />

<article class="mx-auto max-w-3xl px-6 py-10">
	<ArticleMeta post={data.post} author={data.author} />
	<div class="mt-8">
		<FeaturedImage />
	</div>
	<ArticleBody html={data.post.body} />
</article>
```

- [ ] **Step 3: Full visual verification against `Blog item.pdf`**

Run: `pnpm --filter blog dev`, open the homepage, and click into "Legacy Code Is a Love
Letter" (or navigate directly to `/articles/legacy-code-is-a-love-letter`).

Confirm, side-by-side with `Blog item.pdf`:
- Top bar shows "← All columns", the "Digital Dust Library" wordmark, and a "Share" /
  "in" control.
- Eyebrow row shows a blue dot, "Software Dev", "JUL 07", and "10 min".
- Title "Legacy Code Is a Love Letter" and the italic dek render below it.
- Author row shows Sam Okafor's avatar, name, and "Engineering Desk".
- The featured-image placeholder box appears below the author row.
- The article body renders with a large drop-cap "F" on the first paragraph, in the
  accent-blue color.

Then confirm the 404 path: navigate to `/articles/does-not-exist` and confirm the
branded error page from Task 7 renders with "Post not found".

Stop the dev server once confirmed.

- [ ] **Step 4: Type-check, lint, and commit**

Run:
```bash
pnpm --filter blog format
pnpm --filter blog check
pnpm --filter blog lint
```
Expected: no errors.

```bash
git add apps/blog/src/routes/articles
git commit -m "Wire up the article detail route"
```

---

### Task 11: Stub routes (archive, author profile, become-an-author)

**Files:**
- Create: `apps/blog/src/routes/(site)/archive/+page.ts`
- Create: `apps/blog/src/routes/(site)/archive/+page.svelte`
- Create: `apps/blog/src/routes/(site)/authors/[handle]/+page.ts`
- Create: `apps/blog/src/routes/(site)/authors/[handle]/+page.svelte`
- Create: `apps/blog/src/routes/(site)/become-an-author/+page.svelte`

**Interfaces:**
- Consumes: `getAllPosts`, `getPostsByAuthor`, `getAuthorByHandle`, `pillars`
  (Task 2/3, via `$lib/data`); `PostTeaserRow` (Task 6); `Avatar` (Task 5).
- Produces: `/archive`, `/authors/[handle]`, `/become-an-author` routes so
  `SiteHeader`'s nav links resolve instead of 404ing.

- [ ] **Step 1: Write the archive route**

Create `apps/blog/src/routes/(site)/archive/+page.ts`:

```ts
import { getAllPosts } from '$lib/data';
import type { PageLoad } from './$types';

export const load: PageLoad = () => {
	return { posts: getAllPosts() };
};
```

Create `apps/blog/src/routes/(site)/archive/+page.svelte`:

```svelte
<script lang="ts">
	import { getAuthorByHandle, pillars } from '$lib/data';
	import PostTeaserRow from '$lib/components/home/PostTeaserRow.svelte';
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();

	function accentFor(pillarSlug: string) {
		return pillars.find((pillar) => pillar.slug === pillarSlug)?.accent ?? 'red';
	}
</script>

<svelte:head>
	<title>Archive — Digital Dust Library</title>
</svelte:head>

<h1 class="font-display text-3xl font-bold">Archive</h1>
<p class="mt-2 text-ink/60">Every dispatch, most recent first.</p>

<div class="mt-8 max-w-2xl">
	{#each data.posts as post (post.slug)}
		{@const author = getAuthorByHandle(post.authorHandle)}
		{#if author}
			<PostTeaserRow {post} {author} accent={accentFor(post.pillarSlug)} />
		{/if}
	{/each}
</div>
```

- [ ] **Step 2: Write the author profile route**

Create `apps/blog/src/routes/(site)/authors/[handle]/+page.ts`:

```ts
import { getAuthorByHandle, getPostsByAuthor } from '$lib/data';
import { error } from '@sveltejs/kit';
import type { PageLoad } from './$types';

export const load: PageLoad = ({ params }) => {
	const author = getAuthorByHandle(params.handle);

	if (!author) {
		error(404, 'Author not found');
	}

	return { author, posts: getPostsByAuthor(author.handle) };
};
```

Create `apps/blog/src/routes/(site)/authors/[handle]/+page.svelte`:

```svelte
<script lang="ts">
	import { pillars } from '$lib/data';
	import Avatar from '$lib/components/shared/Avatar.svelte';
	import PostTeaserRow from '$lib/components/home/PostTeaserRow.svelte';
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();

	function accentFor(pillarSlug: string) {
		return pillars.find((pillar) => pillar.slug === pillarSlug)?.accent ?? 'red';
	}
</script>

<svelte:head>
	<title>{data.author.name} — Digital Dust Library</title>
</svelte:head>

<div class="flex items-center gap-4">
	<Avatar name={data.author.name} colorClass={data.author.avatarColor} size="md" />
	<div>
		<h1 class="font-display text-2xl font-bold">{data.author.name}</h1>
		<p class="font-label text-xs tracking-widest text-ink/50 uppercase">{data.author.role}</p>
	</div>
</div>

<div class="mt-8 max-w-2xl">
	{#each data.posts as post (post.slug)}
		<PostTeaserRow {post} author={data.author} accent={accentFor(post.pillarSlug)} />
	{/each}
</div>
```

- [ ] **Step 3: Write the become-an-author stub**

Create `apps/blog/src/routes/(site)/become-an-author/+page.svelte`:

```svelte
<svelte:head>
	<title>Become an Author — Digital Dust Library</title>
</svelte:head>

<div class="max-w-xl">
	<h1 class="font-display text-3xl font-bold">Become an author</h1>
	<p class="mt-4 text-ink/70">
		Digital Dust Library is opening up to outside contributors. The application form isn't wired
		up yet — check back soon, or reach out directly in the meantime.
	</p>
</div>
```

- [ ] **Step 4: Visually verify all three stub routes**

Run: `pnpm --filter blog dev` and confirm:
- `/archive` lists all 12 posts as teaser rows, most recent first, each linking to its
  article.
- `/authors/sam-okafor` shows his avatar, name, "Engineering Desk", and his 2 posts.
- `/become-an-author` renders the static message.
- Clicking "Archive" and "Become an Author" in the `SiteHeader` nav lands on these
  pages (not a 404).

Stop the dev server once confirmed.

- [ ] **Step 5: Type-check, lint, and commit**

Run:
```bash
pnpm --filter blog format
pnpm --filter blog check
pnpm --filter blog lint
```
Expected: no errors.

```bash
git add "apps/blog/src/routes/(site)/archive" "apps/blog/src/routes/(site)/authors" "apps/blog/src/routes/(site)/become-an-author"
git commit -m "Add archive, author profile, and become-an-author stub routes"
```

---

### Task 12: Final full verification pass

**Files:** none (verification only).

**Interfaces:** none — this task exercises everything built in Tasks 1–11.

- [ ] **Step 1: Full command suite**

Run, from the repo root:
```bash
pnpm --filter blog format
pnpm --filter blog lint
pnpm --filter blog check
pnpm --filter blog build
```
Expected: all four succeed with no errors.

- [ ] **Step 2: Full manual walkthrough**

Run: `pnpm --filter blog preview` (serves the production build) and open the printed
URL. With `Blog Home.pdf` and `Blog item.pdf` open side by side, walk through:
1. `/` — 3-column grid, correct accents, featured cards, teaser rows, issue banner.
2. Click through to at least one article per pillar and confirm each renders (eyebrow,
   title, dek, author row, featured image, drop-cap body).
3. `/archive`, `/authors/sam-okafor`, `/become-an-author` — all render without error.
4. `/articles/does-not-exist` — branded 404 page.
5. Resize the browser to a narrow (mobile) width and confirm the 3-column grid
   collapses to a single column (from `PillarColumn`'s parent `md:grid-cols-3` in
   Task 8) and the header/nav don't overflow.

Stop the preview server once confirmed.

- [ ] **Step 3: Commit any final fixes**

If Steps 1–2 turned up issues, fix them in the relevant files from earlier tasks, then:

```bash
git add apps/blog
git commit -m "Fix issues found in final verification pass"
```

If no issues were found, skip this step — there's nothing to commit.
