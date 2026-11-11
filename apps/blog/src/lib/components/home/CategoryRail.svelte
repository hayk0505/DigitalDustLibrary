<script lang="ts">
	import { page } from '$app/state';
	import type { Category } from '$lib/data';
	import {
		getCategoryIcon,
		getCategoryTextColor,
		getFolderTone
	} from '$lib/utils/category-visuals';
	import AuthorFile from './AuthorFile.svelte';
	import TurntablePlayer from './TurntablePlayer.svelte';
	let { categories }: { categories: Category[] } = $props();
</script>

<section class="masthead">
	<h1 class="title">DIGITAL DUST <span class="text-accent-red uppercase">Library</span></h1>

	<p class="meta">Field notes on what the internet leaves behind<br /></p>
	<!-- <div class="file">
		<a href="/archive" class="hover:text-accent-red">Archive</a>
		<a href="/become-an-author" class="hover:text-accent-red">Become an Author</a>
	</div> -->
	<div class="barcode" aria-hidden="true"></div>
</section>

<AuthorFile />
<div class="folders-col">
	<span class="folders-heading" id="folders-heading">Categories</span>
	<nav
		class="folders scrollbar-hide flex min-h-0 flex-1 flex-col overflow-y-auto"
		aria-labelledby="folders-heading"
	>
		{#each categories as category (category.slug)}
			{@const Icon = getCategoryIcon(category.slug)}
			{@const tone = getFolderTone(category.folderColor, category.slug)}
			{@const ink = getCategoryTextColor(tone)}
			{@const isActive = page.url.pathname === `/category/${category.slug}`}
			<a
				href="/category/{category.slug}"
				class="folder"
				class:active={isActive}
				style="--tone-raw: {tone}; --ink-on-tone-raw: {ink};"
			>
				{#if isActive}
					<span class="folder-active-dot" aria-hidden="true"></span>
				{/if}
				<span class="folder-icon"><Icon class="h-4 w-4" /></span>
				<span class="folder-label">{category.name}</span>
				<span class="folder-num">{String(category.position).padStart(2, '0')}</span>
			</a>
		{/each}
	</nav>
	<TurntablePlayer />
</div>
