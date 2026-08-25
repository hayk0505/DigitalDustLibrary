<script lang="ts">
	import type { Post } from '$lib/data';
	import MinisSection from './MinisSection.svelte';
	import ShelfSection from './ShelfSection.svelte';
	import MobileFormatToggle from './MobileFormatToggle.svelte';

	let {
		minis,
		shelfPosts,
		showMinis,
		showShelf,
		baseHref,
		viewAllBaseHref,
		typeParam
	}: {
		minis: Post[];
		shelfPosts: Post[];
		showMinis: boolean;
		showShelf: boolean;
		baseHref: string;
		viewAllBaseHref: string;
		typeParam: 'all' | 'mini' | 'shelf';
	} = $props();
</script>

<MobileFormatToggle {baseHref} active={typeParam} />

<div>
	{#if showMinis}
		<MinisSection posts={minis} viewAllHref="{viewAllBaseHref}?type=mini" />
	{/if}
	{#if showShelf}
		<ShelfSection posts={shelfPosts} viewAllHref="{viewAllBaseHref}?type=shelf" />
	{/if}
</div>
