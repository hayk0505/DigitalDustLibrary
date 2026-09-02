<script lang="ts">
	import { page } from '$app/state';
	import BlogTopNav from '$lib/components/layout/BlogTopNav.svelte';
	import MobileCategoryRail from '$lib/components/layout/MobileCategoryRail.svelte';
	import CategorySidebar from '$lib/components/home/CategorySidebar.svelte';
	import ScrollToTopButton from '$lib/components/shared/ScrollToTopButton.svelte';
	import TurntableAudio from '$lib/components/shared/TurntableAudio.svelte';
	import MobileTurntablePlayer from '$lib/components/shared/MobileTurntablePlayer.svelte';
	import { turntable } from '$lib/state/turntable.svelte';
	import { readerMode } from '$lib/state/reader-mode.svelte';
	import { themeState } from '$lib/state/theme.svelte';
	import { savedArticles } from '$lib/state/saved-articles.svelte';
	import { readingHistory } from '$lib/state/reading-history.svelte';

	let { children, data } = $props();

	const isArticlePage = $derived(page.url.pathname.startsWith('/articles/'));
	const isHomePage = $derived(page.url.pathname === '/');
	const isSavedPage = $derived(page.url.pathname === '/saved');

	$effect(() => {
		turntable.setTracks(data.tracks);
	});

	$effect(() => {
		readerMode.load();
	});

	$effect(() => {
		themeState.load();
	});

	$effect(() => {
		savedArticles.load();
	});

	$effect(() => {
		readingHistory.load();
	});
</script>

<div class="flex">
	{#if page.data.categories}
		<CategorySidebar categories={page.data.categories} />
	{/if}
	<div class="flex min-h-screen min-w-0 flex-1 flex-col">
		{#if !isArticlePage}
			<BlogTopNav />
			{#if page.data.categories && !isHomePage && !isSavedPage}
				<MobileCategoryRail categories={page.data.categories} />
			{/if}
		{/if}
		<main class="mx-auto w-full max-w-6xl flex-1 px-6 pb-8 md:pt-6">
			{@render children()}
		</main>
		<footer class="dd-chrome border-t border-ink/10 px-6 py-6">
			<div
				class="mx-auto flex max-w-6xl flex-col gap-2 text-xs text-ink/50 sm:flex-row sm:items-center sm:justify-between"
			>
				<p>
					© {new Date().getFullYear()}
					{page.data.settings.siteTitle}. All rights reserved.
				</p>
				<p class="flex gap-4">
					<a href="/about" class="hover:text-ink">About</a>
					<a href="/privacy" class="hover:text-ink">Privacy</a>
					<a href="/terms" class="hover:text-ink">Terms</a>
				</p>
			</div>
		</footer>
	</div>
</div>

<TurntableAudio />
<div class="dd-chrome">
	<MobileTurntablePlayer />
</div>
<div class="dd-chrome">
	<ScrollToTopButton />
</div>
