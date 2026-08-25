<script lang="ts">
	import type { Category } from '$lib/data';
	import {
		getCategoryIcon,
		getCategoryTabColor,
		getCategoryTextColor
	} from '$lib/utils/category-visuals';

	let { categories, activeSlug }: { categories: Category[]; activeSlug?: string } = $props();
</script>

<nav class="hidden shrink-0 flex-col gap-2 md:flex md:w-56" aria-label="Library categories">
	{#each categories as category (category.slug)}
		{@const Icon = getCategoryIcon(category.slug)}
		{@const tone = category.folderColor ?? getCategoryTabColor(category.slug)}
		{@const ink = getCategoryTextColor(tone)}
		{@const isActive = category.slug === activeSlug}
		<a
			href="/category/{category.slug}"
			class="library-spine flex items-center gap-3 rounded-sm px-3 py-2.5 transition-opacity"
			class:active={isActive}
			style="--tone-raw: {tone}; --ink-on-tone-raw: {ink};"
		>
			<Icon class="h-4 w-4 shrink-0" />
			<span class="min-w-0 flex-1 truncate font-label text-sm tracking-wide uppercase"
				>{category.name}</span
			>
			<span class="shrink-0 font-label text-xs opacity-70">{category.postCount}</span>
		</a>
	{/each}
</nav>
