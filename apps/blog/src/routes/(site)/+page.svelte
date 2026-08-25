<script lang="ts">
	import { page } from '$app/state';
	import SeoHead from '$lib/components/shared/SeoHead.svelte';
	import CategoryColumn from '$lib/components/home/CategoryColumn.svelte';
	import MobileCategoryCarousel from '$lib/components/home/MobileCategoryCarousel.svelte';
	import LibraryLayout from '$lib/components/library/LibraryLayout.svelte';
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();

	const gridColsClass = $derived(
		data.columns.length === 1
			? 'md:grid-cols-1'
			: data.columns.length === 2
				? 'md:grid-cols-2'
				: 'md:grid-cols-3'
	);
</script>

<SeoHead
	title="{page.data.settings.siteTitle} — {page.data.settings.tagline}"
	description={page.data.settings.tagline}
	url={page.url.href}
	appendSiteName={false}
/>

<MobileCategoryCarousel columns={data.allColumns} />

<div class="hidden md:block">
	<LibraryLayout
		minis={data.minis}
		shelfPosts={data.shelfPosts}
		showMinis={data.showMinis}
		showShelf={data.showShelf}
		baseHref="/"
		viewAllBaseHref="/articles"
		typeParam={data.typeParam}
	/>
</div>

<div class="mt-12 hidden md:block">
	<div class="flex items-center gap-4">
		<div class="h-px flex-1 bg-ink/10"></div>
		<span class="font-label text-xs tracking-widest text-ink/60 uppercase">Categories</span>
		<div class="h-px flex-1 bg-ink/10"></div>
	</div>

	<div class="mt-8 grid gap-10 {gridColsClass}">
		{#each data.columns as column (column.category.slug)}
			<CategoryColumn category={column.category} posts={column.posts} />
		{/each}
	</div>
</div>
