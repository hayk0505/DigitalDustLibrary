<script lang="ts">
	import type { Post } from '$lib/data';
	import MasonryCard from './MasonryCard.svelte';

	let { posts, color }: { posts: Post[]; color: string } = $props();
</script>

<div class="columns-2 gap-4 sm:columns-3 lg:columns-4">
	{#each posts as post (post.slug)}
		<MasonryCard {post} {color} />
	{/each}
</div>
