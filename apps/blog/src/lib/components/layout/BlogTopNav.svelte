<script lang="ts">
	import { page } from '$app/state';
	import { goto } from '$app/navigation';
	import { ChevronLeft, ChevronRight } from 'lucide-svelte';
	import logo from '$lib/assets/logo.svg';
	import ThemeToggleButton from './ThemeToggleButton.svelte';
	import MobileMenuButton from './MobileMenuButton.svelte';
	import SearchControl from '$lib/components/search/SearchControl.svelte';

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
</script>

<div class="border-b border-ink/10">
	<div class="hidden items-center justify-between px-6 py-3 font-label text-xs tracking-widest uppercase md:flex sm:px-10">
		<div class="flex items-center gap-6">
			<a href="/" class="flex items-center leading-tight">
				<img src={logo} alt="" class="h-8 w-8" />
			</a>
			<nav class="flex items-center gap-6">
				<a href="/become-an-author" class="underline hover:text-accent-red">Become an author</a>
			</nav>
		</div>
		<div class="flex items-center gap-4">
			{#if showPager}
				<div class="flex items-center gap-1">
					<button
						type="button"
						disabled={catPage === 0}
						onclick={() => goToPage(catPage - 1)}
						aria-label="Previous categories"
						class="border border-ink/20 px-2 py-1.5 hover:border-ink/40 disabled:opacity-30"
					>
						<ChevronLeft class="h-4 w-4" aria-hidden="true" />
					</button>
					<span>{catPage + 1}/{totalCatPages}</span>
					<button
						type="button"
						disabled={catPage >= totalCatPages - 1}
						onclick={() => goToPage(catPage + 1)}
						aria-label="Next categories"
						class="border border-ink/20 px-2 py-1.5 hover:border-ink/40 disabled:opacity-30"
					>
						<ChevronRight class="h-4 w-4" aria-hidden="true" />
					</button>
				</div>
			{/if}
			<ThemeToggleButton />
			<SearchControl />
		</div>
	</div>

	<div class="flex items-center justify-between px-4 py-3 md:hidden">
		<a href="/" class="flex items-center leading-tight">
			<img src={logo} alt="" class="h-8 w-8" />
		</a>
		<div class="flex items-center gap-4">
			<ThemeToggleButton iconClass="h-5 w-5" />
			<SearchControl />
			<MobileMenuButton iconClass="h-6 w-6" />
		</div>
	</div>
</div>
