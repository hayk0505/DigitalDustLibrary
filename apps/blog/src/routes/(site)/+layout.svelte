<script lang="ts">
	import { page } from '$app/state';
	import BlogTopNav from '$lib/components/layout/BlogTopNav.svelte';
	import MobileCategoryRail from '$lib/components/layout/MobileCategoryRail.svelte';
	import CategorySidebar from '$lib/components/home/CategorySidebar.svelte';
	import ScrollToTopButton from '$lib/components/shared/ScrollToTopButton.svelte';

	let { children } = $props();
	
	const isArticlePage = $derived(page.url.pathname.startsWith('/articles/'));
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

<ScrollToTopButton />
