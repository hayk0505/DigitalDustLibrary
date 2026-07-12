<script lang="ts">
	import { getAuthorByHandle, type Pillar, type Post } from '$lib/data';
	import FeaturedPostCard from './FeaturedPostCard.svelte';
	import PillarBadge from './PillarBadge.svelte';
	import PostTeaserRow from './PostTeaserRow.svelte';

	let { pillar, posts }: { pillar: Pillar; posts: Post[] } = $props();

	const featuredPost = $derived(posts.find((post) => post.featured));
	const restPosts = $derived(posts.filter((post) => !post.featured));
</script>

<section>
	<PillarBadge {pillar} postCount={posts.length} />

	{#if featuredPost}
		{@const featuredAuthor = getAuthorByHandle(featuredPost.authorHandle)}
		{#if featuredAuthor}
			<FeaturedPostCard post={featuredPost} author={featuredAuthor} accent={pillar.accent} />
		{/if}
	{/if}

	{#each restPosts as post (post.slug)}
		{@const author = getAuthorByHandle(post.authorHandle)}
		{#if author}
			<PostTeaserRow {post} {author} accent={pillar.accent} />
		{/if}
	{/each}

	<p class="mt-4 text-center font-label text-xs tracking-widest text-ink/40 uppercase">
		— End of column —
	</p>
</section>
