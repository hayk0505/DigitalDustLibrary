<script lang="ts">
	import { page } from '$app/state';
	import { goto } from '$app/navigation';
	import SeoHead from '$lib/components/shared/SeoHead.svelte';
	import IssueBanner from '$lib/components/home/IssueBanner.svelte';
	import CategoryColumn from '$lib/components/home/CategoryColumn.svelte';
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();

	const gridColsClass = $derived(
		data.columns.length === 1 ? 'md:grid-cols-1' : data.columns.length === 2 ? 'md:grid-cols-2' : 'md:grid-cols-3'
	);

	function goToPage(nextPage: number) {
		const url = new URL(page.url);
		url.searchParams.set('catPage', String(nextPage));
		goto(url, { noScroll: true });
	}
</script>

<SeoHead
	title="Digital Dust Library — Field notes on what the internet leaves behind"
	description="Field notes on what the internet leaves behind."
	url={page.url.href}
/>

<IssueBanner
	volumeLabel="Vol. 04 — July 2026"
	dispatchCount={data.totalCount}
	showPager={data.hasMultipleCatPages}
	catPage={data.catPage}
	totalCatPages={data.totalCatPages}
	onPrev={() => goToPage(data.catPage - 1)}
	onNext={() => goToPage(data.catPage + 1)}
/>

<div class="mt-8 grid gap-10 {gridColsClass}">
	{#each data.columns as column (column.category.slug)}
		<CategoryColumn category={column.category} posts={column.posts} />
	{/each}
</div>
