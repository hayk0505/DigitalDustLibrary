<script lang="ts">
	import { pillars } from '$lib/data';
	import Avatar from '$lib/components/shared/Avatar.svelte';
	import PostTeaserRow from '$lib/components/home/PostTeaserRow.svelte';
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();

	function accentFor(pillarSlug: string) {
		return pillars.find((pillar) => pillar.slug === pillarSlug)?.accent ?? 'red';
	}
</script>

<svelte:head>
	<title>{data.author.name} — Digital Dust Library</title>
</svelte:head>

<div class="flex items-center gap-4">
	<Avatar name={data.author.name} colorClass={data.author.avatarColor} size="md" />
	<div>
		<h1 class="font-display text-2xl font-bold">{data.author.name}</h1>
		<p class="font-label text-xs tracking-widest text-ink/50 uppercase">{data.author.role}</p>
	</div>
</div>

<div class="mt-8 max-w-2xl">
	{#each data.posts as post (post.slug)}
		<PostTeaserRow {post} author={data.author} accent={accentFor(post.pillarSlug)} />
	{/each}
</div>
