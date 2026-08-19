<script lang="ts">
	import { Menu, Share2, ChevronLeft, ChevronRight } from 'lucide-svelte';
	import type { Post } from '$lib/data';
	import { createReadingProgress } from '$lib/utils/reading-progress.svelte';
	import ReaderContentsPanel from './ReaderContentsPanel.svelte';

	let {
		shareUrl,
		prevPost,
		nextPost
	}: { shareUrl: string; prevPost: Post | null; nextPost: Post | null } = $props();

	const progress = createReadingProgress();

	const linkedInHref = $derived(
		`https://www.linkedin.com/sharing/share-offsite/?url=${encodeURIComponent(shareUrl)}`
	);
</script>

<nav class="dd-reader-navbar is-mobile" aria-label="Article navigation">
	<ReaderContentsPanel class="dd-reader-navbar-link">
		<Menu class="h-3.5 w-3.5" aria-hidden="true" />
		Contents
	</ReaderContentsPanel>

	<!-- Icon-only prev/next flanking the reading percentage, not the
	     text-labelled links ReaderNavBarDesktop uses — five full-width
	     items (Contents, Prev, %, Next, Share) don't fit a mobile row, so
	     prev/next collapse to arrows around the one figure they're already
	     next to conceptually. -->
	<div class="dd-reader-navbar-progress-group">
		{#if prevPost}
			<a href="/articles/{prevPost.slug}" class="dd-reader-navbar-arrow" aria-label="Previous article">
				<ChevronLeft class="h-3.5 w-3.5" aria-hidden="true" />
			</a>
		{:else}
			<span class="dd-reader-navbar-arrow" aria-disabled="true" aria-label="Previous article">
				<ChevronLeft class="h-3.5 w-3.5" aria-hidden="true" />
			</span>
		{/if}

		<span class="dd-reader-navbar-progress">{Math.round(progress.value)}% Read</span>

		{#if nextPost}
			<a href="/articles/{nextPost.slug}" class="dd-reader-navbar-arrow" aria-label="Next article">
				<ChevronRight class="h-3.5 w-3.5" aria-hidden="true" />
			</a>
		{:else}
			<span class="dd-reader-navbar-arrow" aria-disabled="true" aria-label="Next article">
				<ChevronRight class="h-3.5 w-3.5" aria-hidden="true" />
			</span>
		{/if}
	</div>

	<a href={linkedInHref} target="_blank" rel="noopener noreferrer" class="dd-reader-navbar-link">
		<Share2 class="h-3.5 w-3.5" aria-hidden="true" />
		Share
	</a>
</nav>
