<script lang="ts">
	import type { Category, Post } from '$lib/data';
	import { getCategoryTabColor } from '$lib/utils/category-visuals';
	import FeaturedPostCard from './FeaturedPostCard.svelte';
	import CategoryBadge from './CategoryBadge.svelte';
	import PostTeaserRow from './PostTeaserRow.svelte';

	let { category, posts }: { category: Category; posts: Post[] } = $props();

	const featuredPost = $derived(posts[0]);
	const restPosts = $derived(posts.slice(1));
	const accentColor = $derived(category.folderColor ?? getCategoryTabColor(category.slug));
</script>

<section class="border-r border-ink/10 pr-6 last:border-r-0 last:pr-0">
	<CategoryBadge {category} postCount={posts.length} />

	{#if featuredPost}
		<FeaturedPostCard post={featuredPost} color={accentColor} />
	{/if}

	{#each restPosts as post (post.slug)}
		<PostTeaserRow {post} color={accentColor} />
	{/each}

	<p class="mt-4 text-center font-label text-xs tracking-widest text-ink/40 uppercase">
		— End of column —
	</p>
</section>
