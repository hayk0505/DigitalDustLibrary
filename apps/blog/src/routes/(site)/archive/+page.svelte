<script lang="ts">
	import { page } from '$app/state';
	import { getPillarBySlug } from '$lib/data';
	import SeoHead from '$lib/components/shared/SeoHead.svelte';
	import PostTeaserRow from '$lib/components/home/PostTeaserRow.svelte';
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();

	function accentFor(pillarSlug: string) {
		return getPillarBySlug(pillarSlug)?.accent ?? 'red';
	}
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
		<PostTeaserRow {post} accent={accentFor(post.pillarSlug)} />
	{/each}
</div>
