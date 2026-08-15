<script lang="ts">
	import type { Post } from '$lib/data';
	import ArticleGridCard from './ArticleGridCard.svelte';

	let { posts, color }: { posts: Post[]; color: string } = $props();
</script>

<div class="grid grid-cols-2 gap-3 sm:gap-4 lg:grid-cols-4">
	{#each posts as post (post.slug)}
		<ArticleGridCard {post} {color} />
	{/each}
</div>
