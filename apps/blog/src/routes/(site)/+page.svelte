<script lang="ts">
	import { page } from '$app/state';
	import SeoHead from '$lib/components/shared/SeoHead.svelte';
	import CategoryColumn from '$lib/components/home/CategoryColumn.svelte';
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();

	const gridColsClass = $derived(
		data.columns.length === 1 ? 'md:grid-cols-1' : data.columns.length === 2 ? 'md:grid-cols-2' : 'md:grid-cols-3'
	);
</script>

<SeoHead
	title="Digital Dust Library — Field notes on what the internet leaves behind"
	description="Field notes on what the internet leaves behind."
	url={page.url.href}
/>

<div class="grid gap-10 {gridColsClass}">
	{#each data.columns as column (column.category.slug)}
		<CategoryColumn category={column.category} posts={column.posts} />
	{/each}
</div>
