<script lang="ts">
	import type { Category } from '$lib/data';
	import { getCategoryIcon, getCategoryTabColor, getCategoryTextColor } from '$lib/utils/category-visuals';

	let { categories }: { categories: Category[] } = $props();
</script>

<nav
	class="scrollbar-hide flex gap-2 overflow-x-auto border-b border-ink/10 bg-paper px-4 py-3 md:hidden"
	aria-label="Categories"
>
	{#each categories as category (category.slug)}
		{@const Icon = getCategoryIcon(category.slug)}
		{@const tone = category.folderColor ?? getCategoryTabColor(category.slug)}
		{@const ink = getCategoryTextColor(tone)}
		<a
			href="/category/{category.slug}"
			class="mobile-category-chip flex shrink-0 items-center gap-1.5 rounded-full px-3 py-1.5 font-label text-xs tracking-widest uppercase"
			style="--tone-raw: {tone}; --ink-on-tone-raw: {ink};"
		>
			<Icon class="h-3.5 w-3.5 shrink-0" />
			<span class="mobile-category-chip-label">{category.name}</span>
			<span class="opacity-70">{String(category.position).padStart(2, '0')}</span>
		</a>
	{/each}
</nav>
