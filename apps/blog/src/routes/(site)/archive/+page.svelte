<script lang="ts">
	import { page } from '$app/state';
	import SeoHead from '$lib/components/shared/SeoHead.svelte';
	import PostTeaserRow from '$lib/components/home/PostTeaserRow.svelte';
	import { getCategoryTabColor } from '$lib/utils/category-visuals';
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();
</script>

<SeoHead
	title="Archive — Digital Dust Library"
	description="Every dispatch, most recent first."
	url={page.url.href}
/>

<h1 class="font-display text-3xl font-bold">Archive</h1>
<p class="mt-2 text-ink/60">Every dispatch, most recent first.</p>

<div class="mt-8 max-w-2xl">
	{#each data.posts as post (post.slug)}
		{@const accentColor = post.categoryFolderColor ?? getCategoryTabColor(post.categorySlug)}
		<PostTeaserRow {post} color={accentColor} />
	{/each}
</div>
