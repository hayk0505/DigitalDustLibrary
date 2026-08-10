<script lang="ts">
	import type { Pillar, Post } from '$lib/data';
	import FeaturedPostCard from './FeaturedPostCard.svelte';
	import PillarBadge from './PillarBadge.svelte';
	import PostTeaserRow from './PostTeaserRow.svelte';

	let { pillar, posts }: { pillar: Pillar; posts: Post[] } = $props();

	// posts arrives newest-first (the API already sorts this way) — index 0
	// is "featured" per sub-project 1's design decision (no stored flag,
	// just a position convention).
	const featuredPost = $derived(posts[0]);
	const restPosts = $derived(posts.slice(1));
</script>

<section>
	<PillarBadge {pillar} postCount={posts.length} />

	{#if featuredPost}
		<FeaturedPostCard post={featuredPost} accent={pillar.accent} />
	{/if}

	{#each restPosts as post (post.slug)}
		<PostTeaserRow {post} accent={pillar.accent} />
	{/each}

	<p class="mt-4 text-center font-label text-xs tracking-widest text-ink/40 uppercase">
		— End of column —
	</p>
</section>
