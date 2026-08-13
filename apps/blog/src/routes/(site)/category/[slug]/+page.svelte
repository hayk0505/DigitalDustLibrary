<script lang="ts">
	import { page } from '$app/state';
	import SeoHead from '$lib/components/shared/SeoHead.svelte';
	import PostTeaserRow from '$lib/components/home/PostTeaserRow.svelte';
	import MasonryGrid from '$lib/components/category/MasonryGrid.svelte';
	import ViewToggle from '$lib/components/category/ViewToggle.svelte';
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();
	let view = $state<'grid' | 'list'>('grid');
</script>

<SeoHead
	title="{data.category.name} — Digital Dust Library"
	description={data.category.description}
	url={page.url.href}
/>

<div class="flex flex-wrap items-end justify-between gap-4">
	<div>
		<h1 class="font-display text-3xl font-bold uppercase">{data.category.name}</h1>
		<p class="mt-2 text-ink/60">{data.category.description}</p>
		<p class="mt-1 font-label text-xs tracking-widest text-ink/40 uppercase">
			{data.posts.length}
			{data.posts.length === 1 ? 'post' : 'posts'}
		</p>
	</div>
	<ViewToggle bind:view />
</div>

<div class="mt-8">
	{#if view === 'list'}
		<div class="max-w-2xl">
			{#each data.posts as post (post.slug)}
				<PostTeaserRow {post} color={data.category.color} />
			{/each}
		</div>
	{:else}
		<MasonryGrid posts={data.posts} color={data.category.color} />
	{/if}
</div>
