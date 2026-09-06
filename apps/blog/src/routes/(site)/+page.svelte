<script lang="ts">
	import { page } from '$app/state';
	import { goto } from '$app/navigation';
	import { ChevronLeft, ChevronRight } from 'lucide-svelte';
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

	function goToCatPage(nextPage: number) {
		const url = new URL(page.url);
		url.searchParams.set('catPage', String(nextPage));
		goto(url, { noScroll: true });
	}
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
		{#if data.totalCatPages > 1}
			<div class="flex items-center gap-1 font-label text-xs tracking-widest uppercase">
				<button
					type="button"
					disabled={data.catPage === 0}
					onclick={() => goToCatPage(data.catPage - 1)}
					aria-label="Previous categories"
					class="cursor-pointer border border-ink/20 px-2 py-1.5 hover:border-ink/40 disabled:cursor-not-allowed disabled:opacity-30"
				>
					<ChevronLeft class="h-4 w-4" aria-hidden="true" />
				</button>
				<span>{data.catPage + 1}/{data.totalCatPages}</span>
				<button
					type="button"
					disabled={data.catPage >= data.totalCatPages - 1}
					onclick={() => goToCatPage(data.catPage + 1)}
					aria-label="Next categories"
					class="cursor-pointer border border-ink/20 px-2 py-1.5 hover:border-ink/40 disabled:cursor-not-allowed disabled:opacity-30"
				>
					<ChevronRight class="h-4 w-4" aria-hidden="true" />
				</button>
			</div>
		{/if}
		<div class="h-px flex-1 bg-ink/10"></div>
	</div>

	<div class="mt-8 grid gap-10 {gridColsClass}">
		{#each data.columns as column (column.category.slug)}
			<CategoryColumn category={column.category} posts={column.posts} />
		{/each}
	</div>
</div>
