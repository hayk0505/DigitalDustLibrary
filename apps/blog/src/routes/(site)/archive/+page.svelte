<script lang="ts">
	import { getAuthorByHandle, getPillarBySlug } from '$lib/data';
	import PostTeaserRow from '$lib/components/home/PostTeaserRow.svelte';
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();

	function accentFor(pillarSlug: string) {
		return getPillarBySlug(pillarSlug)?.accent ?? 'red';
	}
</script>

<svelte:head>
	<title>Archive — Digital Dust Library</title>
</svelte:head>

<h1 class="font-display text-3xl font-bold">Archive</h1>
<p class="mt-2 text-ink/60">Every dispatch, most recent first.</p>

<div class="mt-8 max-w-2xl">
	{#each data.posts as post (post.slug)}
		{@const author = getAuthorByHandle(post.authorHandle)}
		{#if author}
			<PostTeaserRow {post} {author} accent={accentFor(post.pillarSlug)} />
		{/if}
	{/each}
</div>
