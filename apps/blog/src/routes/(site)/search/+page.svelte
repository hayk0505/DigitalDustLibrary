<script lang="ts">
	import { page } from '$app/state';
	import SeoHead from '$lib/components/shared/SeoHead.svelte';
	import SearchResultItem from '$lib/components/search/SearchResultItem.svelte';
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();
</script>

<SeoHead
	title="Search — Digital Dust Library"
	description="Search the Digital Dust Library."
	url={page.url.href}
/>

<span class="font-label text-xs tracking-widest text-ink/50 uppercase">Search Library</span>
<h1 class="mt-1 font-display text-3xl font-bold">{data.query || 'Enter a search term'}</h1>

{#if data.query}
	<p class="mt-2 text-ink/60">
		{data.results.length}
		{data.results.length === 1 ? 'result' : 'results'}
	</p>

	<div class="mt-6 max-w-2xl">
		{#each data.results as post, i (post.slug)}
			<SearchResultItem {post} index={i} />
		{/each}
		{#if data.results.length === 0}
			<p class="text-ink/60">No results for "{data.query}".</p>
		{/if}
	</div>
{/if}
