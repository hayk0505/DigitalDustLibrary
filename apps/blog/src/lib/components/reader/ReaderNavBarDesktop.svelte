<script lang="ts">
	import { ChevronLeft, ChevronRight } from 'lucide-svelte';
	import type { Post } from '$lib/data';
	import { createReadingProgress } from '$lib/utils/reading-progress.svelte';

	let { prevPost, nextPost }: { prevPost: Post | null; nextPost: Post | null } = $props();

	const progress = createReadingProgress();
</script>

<nav class="dd-reader-navbar is-desktop" aria-label="Article navigation">
	{#if prevPost}
		<a href="/articles/{prevPost.slug}" class="dd-reader-navbar-link">
			<ChevronLeft class="h-3.5 w-3.5" aria-hidden="true" />
			Prev Article
		</a>
	{:else}
		<span class="dd-reader-navbar-link" aria-disabled="true">
			<ChevronLeft class="h-3.5 w-3.5" aria-hidden="true" />
			Prev Article
		</span>
	{/if}

	<span class="dd-reader-navbar-progress">{Math.round(progress.value)}% Read</span>

	{#if nextPost}
		<a href="/articles/{nextPost.slug}" class="dd-reader-navbar-link">
			Next Article
			<ChevronRight class="h-3.5 w-3.5" aria-hidden="true" />
		</a>
	{:else}
		<span class="dd-reader-navbar-link" aria-disabled="true">
			Next Article
			<ChevronRight class="h-3.5 w-3.5" aria-hidden="true" />
		</span>
	{/if}
</nav>
