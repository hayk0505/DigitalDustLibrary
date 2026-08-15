<script lang="ts">
	import { page } from '$app/state';
	import { goto } from '$app/navigation';
	import { Menu } from 'lucide-svelte';
	import logo from '$lib/assets/logo.svg';
	import MobileMenuDrawer from './MobileMenuDrawer.svelte';

	// Mirrors the CATEGORIES_PER_PAGE/catPage math in (site)/+page.ts — both
	// read the same categories list and the same ?catPage= URL param, so
	// they can't drift out of sync despite computing independently. Kept
	// independent (rather than threaded down as props) because this nav
	// lives in the shared layout while the pagination is homepage-only.
	const CATEGORIES_PER_PAGE = 3;

	const categories = $derived(page.data.categories ?? []);
	const totalCatPages = $derived(Math.max(1, Math.ceil(categories.length / CATEGORIES_PER_PAGE)));
	const catPage = $derived(
		Math.min(Math.max(0, Number(page.url.searchParams.get('catPage') ?? '0') || 0), totalCatPages - 1)
	);
	const showPager = $derived(page.url.pathname === '/' && categories.length > CATEGORIES_PER_PAGE);

	function goToPage(nextPage: number) {
		const url = new URL(page.url);
		url.searchParams.set('catPage', String(nextPage));
		goto(url, { noScroll: true });
	}

	let drawerOpen = $state(false);
</script>

<div class="border-b border-ink/10">
	<!-- Desktop / tablet: full nav row, no hamburger. -->
	<div class="hidden items-center justify-between px-6 py-3 font-label text-xs tracking-widest uppercase md:flex sm:px-10">
		<div class="flex items-center gap-6">
			<a href="/" class="flex items-center leading-tight">
				<img src={logo} alt="" class="h-8 w-8" />
			</a>
			<nav class="flex items-center gap-6">
				<a href="/archive" class="underline hover:text-accent-red">Archive</a>
				<a href="/become-an-author" class="underline hover:text-accent-red">Become an author</a>
			</nav>
		</div>
		{#if showPager}
			<div class="flex items-center gap-1">
				<button
					type="button"
					disabled={catPage === 0}
					onclick={() => goToPage(catPage - 1)}
					class="border border-ink/20 px-3 py-1.5 hover:border-ink/40 disabled:opacity-30"
				>
					Prev categories
				</button>
				<span>{catPage + 1}/{totalCatPages}</span>
				<button
					type="button"
					disabled={catPage >= totalCatPages - 1}
					onclick={() => goToPage(catPage + 1)}
					class="border border-ink/20 px-3 py-1.5 hover:border-ink/40 disabled:opacity-30"
				>
					Next categories
				</button>
			</div>
		{/if}
	</div>

	<!-- Mobile: compact header — logo left, hamburger right. Archive/Become
	     an author/theme move into the drawer (see acceptance criteria: no
	     desktop nav links directly in the mobile header). -->
	<div class="flex items-center justify-between px-4 py-3 md:hidden">
		<a href="/" class="flex items-center leading-tight">
			<img src={logo} alt="" class="h-8 w-8" />
		</a>
		<button
			type="button"
			onclick={() => (drawerOpen = true)}
			aria-label="Open menu"
			class="text-ink/70 hover:text-ink"
		>
			<Menu class="h-6 w-6" />
		</button>
	</div>
</div>

<MobileMenuDrawer bind:open={drawerOpen} />
