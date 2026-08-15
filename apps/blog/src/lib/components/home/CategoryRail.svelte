<script lang="ts">
	import { page } from '$app/state';
	import type { Category } from '$lib/data';
	import { getCategoryIcon, getCategoryTabColor, getCategoryTextColor } from '$lib/utils/category-visuals';
	import AuthorFile from './AuthorFile.svelte';
	import ThemeToggle from '../shared/ThemeToggle.svelte';

	let { categories }: { categories: Category[] } = $props();
</script>

<section class="masthead">
	<h1 class="title">DIGITAL DUST <span class="text-accent-red uppercase">Library</span></h1>

	<p class="meta">Field notes on what the internet leaves behind<br>Type. Image. Emotion</p>
	
	<ThemeToggle class="theme-toggle-spine text-sm" />
	<div class="file"></div><div class="barcode" aria-hidden="true"></div>
</section>

<AuthorFile />

<nav class="folders scrollbar-hide flex h-full w-28 shrink-0 flex-col overflow-y-auto" aria-label="Categories">
	<div class="folder-header">
		<span class="folder-header-label">Archive Log</span>
		<span class="folder-header-num">01-A</span>
	</div>
	{#each categories as category (category.slug)}
		{@const Icon = getCategoryIcon(category.slug)}
		{@const tone = category.folderColor ?? getCategoryTabColor(category.slug)}
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
