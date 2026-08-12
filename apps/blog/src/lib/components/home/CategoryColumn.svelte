<script lang="ts">
	import type { Category, Post } from '$lib/data';
	import FeaturedPostCard from './FeaturedPostCard.svelte';
	import CategoryBadge from './CategoryBadge.svelte';
	import PostTeaserRow from './PostTeaserRow.svelte';

	let { category, posts }: { category: Category; posts: Post[] } = $props();

	const featuredPost = $derived(posts[0]);
	const restPosts = $derived(posts.slice(1));
</script>

<section>
	<CategoryBadge {category} postCount={posts.length} />

	{#if featuredPost}
		<FeaturedPostCard post={featuredPost} color={category.color} />
	{/if}

	{#each restPosts as post (post.slug)}
		<PostTeaserRow {post} color={category.color} />
	{/each}

	<p class="mt-4 text-center font-label text-xs tracking-widest text-ink/40 uppercase">
		— End of column —
	</p>
</section>
