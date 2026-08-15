<script lang="ts">
	import { page } from '$app/state';
	import SeoHead from '$lib/components/shared/SeoHead.svelte';
	import PostTeaserRow from '$lib/components/home/PostTeaserRow.svelte';
	import CategoryGrid from '$lib/components/category/CategoryGrid.svelte';
	import ViewToggle from '$lib/components/category/ViewToggle.svelte';
	import { getCategoryTabColor } from '$lib/utils/category-visuals';
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();

	const STORAGE_KEY = 'dd-blog-category-view';
	let view = $state<'grid' | 'list'>(
		(typeof localStorage !== 'undefined' && (localStorage.getItem(STORAGE_KEY) as 'grid' | 'list')) || 'grid'
	);

	$effect(() => {
		localStorage.setItem(STORAGE_KEY, view);
	});

	const accentColor = $derived(data.category.folderColor ?? getCategoryTabColor(data.category.slug));
</script>

<SeoHead
	title="{data.category.name} — Digital Dust Library"
	description={data.category.description}
	url={page.url.href}
/>

<div class="flex flex-wrap items-end justify-between gap-4">
	<div>
		<div class="flex items-center gap-2 font-label text-xs tracking-widest uppercase">
			<span class="accent-bg inline-block h-2.5 w-2.5 rounded-full" style="--accent: {accentColor}"></span>
			<span class="text-ink/60">Column {String(data.category.position).padStart(2, '0')}</span>
			<span class="text-ink/30">·</span>
			<span class="text-ink/50">
				{data.categoryPosts.length}
				{data.categoryPosts.length === 1 ? 'post' : 'posts'}
			</span>
		</div>
		<h1 class="mt-1 font-label text-3xl uppercase">{data.category.name}</h1>
		<p class="mt-2 max-w-xl text-ink/60">{data.category.description}</p>
		<div class="accent-bg mt-3 h-1 w-10" style="--accent: {accentColor}"></div>
	</div>
	<ViewToggle bind:view />
</div>

<div class="mt-8">
	{#if data.categoryPosts.length === 0}
		<p class="archive-empty py-16 text-center font-label text-xs uppercase">
			— No dispatches in this file —
		</p>
	{:else if view === 'list'}
		<div>
			{#each data.categoryPosts as post (post.slug)}
				<PostTeaserRow {post} color={accentColor} showImage />
			{/each}
		</div>
	{:else}
		<CategoryGrid posts={data.categoryPosts} color={accentColor} />
	{/if}
</div>
