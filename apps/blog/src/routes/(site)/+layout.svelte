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

	let { children, data } = $props();

	const isArticlePage = $derived(page.url.pathname.startsWith('/articles/'));

	// Client-only by construction — Svelte 5 effects never run during SSR —
	// so this can't leak one user's playlist into another's SSR output on the
	// shared Workers isolate. See turntable.svelte.ts's class comment.
	$effect(() => {
		turntable.setTracks(data.tracks);
	});

	// Brings readerMode.active in sync with what app.html's inline script
	// already decided (and already applied via the .dd-chrome CSS rule
	// below, pre-hydration) — this component's own {#if}s never gate on
	// readerMode.active directly, so there's nothing here for a flash to
	// affect; see reader-mode.svelte.ts.
	$effect(() => {
		readerMode.load();
	});
</script>

<div class="flex">
	{#if page.data.categories}
		<CategorySidebar categories={page.data.categories} />
	{/if}
	<div class="min-w-0 flex-1">
		{#if !isArticlePage}
			<BlogTopNav />
			{#if page.data.categories}
				<MobileCategoryRail categories={page.data.categories} />
			{/if}
		{/if}
		<main class="mx-auto max-w-6xl px-6 pb-8 md:pt-6">
			{@render children()}
		</main>
	</div>
</div>

<TurntableAudio />
<div class="dd-chrome">
	<MobileTurntablePlayer />
</div>
<div class="dd-chrome">
	<ScrollToTopButton />
</div>
