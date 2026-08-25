<script lang="ts">
	import { page } from '$app/state';
	import SeoHead from '$lib/components/shared/SeoHead.svelte';
	import PostTeaserRow from '$lib/components/home/PostTeaserRow.svelte';
	import CategoryGrid from '$lib/components/category/CategoryGrid.svelte';
	import ViewToggle from '$lib/components/category/ViewToggle.svelte';
	import MobileFormatToggle from '$lib/components/library/MobileFormatToggle.svelte';
	import { getCategoryTabColor } from '$lib/utils/category-visuals';
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();

	const STORAGE_KEY = 'dd-blog-category-view';
	let view = $state<'grid' | 'list'>(
		(typeof localStorage !== 'undefined' &&
			(localStorage.getItem(STORAGE_KEY) as 'grid' | 'list')) ||
			'grid'
	);

	$effect(() => {
		localStorage.setItem(STORAGE_KEY, view);
	});
</script>

<SeoHead
	title="Articles"
	description="Every Mini and Shelf piece, across every column."
	url={page.url.href}
/>

<div class="flex flex-wrap items-end justify-between gap-4">
	<div>
		<h1 class="font-label text-3xl uppercase">Articles</h1>
		<p class="mt-2 max-w-xl text-ink/60">Every dispatch, across every column.</p>
	</div>
	<ViewToggle bind:view />
</div>

<div class="mt-6">
	<MobileFormatToggle baseHref="/articles" active={data.typeParam} />
</div>

<div class="mt-8">
	{#if data.posts.length === 0}
		<p class="archive-empty py-16 text-center font-label text-xs uppercase">
			— No dispatches here —
		</p>
	{:else if view === 'list'}
		<div>
			{#each data.posts as post (post.slug)}
				<PostTeaserRow
					{post}
					color={post.categoryFolderColor ?? getCategoryTabColor(post.categorySlug)}
					showImage
				/>
			{/each}
		</div>
	{:else}
		<CategoryGrid posts={data.posts} />
	{/if}
</div>
