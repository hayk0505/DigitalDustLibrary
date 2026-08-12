<script lang="ts">
	import { page } from '$app/state';
	import { getAvatarColor } from '$lib/utils/avatar-color';
	import Avatar from '$lib/components/shared/Avatar.svelte';
	import SeoHead from '$lib/components/shared/SeoHead.svelte';
	import PostTeaserRow from '$lib/components/home/PostTeaserRow.svelte';
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();
</script>

<SeoHead
	title="{data.author.name} — Digital Dust Library"
	description="Posts by {data.author.name} on Digital Dust Library"
	url={page.url.href}
/>

<div class="flex items-center gap-4">
	<Avatar name={data.author.name} colorClass={getAvatarColor(data.author.handle)} size="md" />
	<div>
		<h1 class="font-display text-2xl font-bold">{data.author.name}</h1>
	</div>
</div>

<div class="mt-8 max-w-2xl">
	{#each data.posts as post (post.slug)}
		<PostTeaserRow {post} color={post.categoryColor} />
	{/each}
</div>
