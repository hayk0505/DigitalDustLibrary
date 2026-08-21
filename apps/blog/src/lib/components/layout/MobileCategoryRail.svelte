<script lang="ts">
	import { page } from '$app/state';
	import type { Category } from '$lib/data';
	import { getCategoryIcon, getCategoryTabColor, getCategoryTextColor } from '$lib/utils/category-visuals';

	let { categories }: { categories: Category[] } = $props();
</script>

<nav
	class="scrollbar-hide flex gap-1 overflow-x-auto border-b border-ink/10 bg-paper px-2 py-3 md:hidden"
	aria-label="Categories"
>
	{#each categories as category (category.slug)}
		{@const Icon = getCategoryIcon(category.slug)}
		{@const tone = category.folderColor ?? getCategoryTabColor(category.slug)}
		{@const ink = getCategoryTextColor(tone)}
		{@const isActive = page.url.pathname === `/category/${category.slug}`}
		<a
			href="/category/{category.slug}"
			class="mobile-category-tab"
			class:active={isActive}
			aria-current={isActive ? 'page' : undefined}
			style="--tone-raw: {tone}; --ink-on-tone-raw: {ink};"
		>
			<span class="mobile-category-tab-icon">
				<Icon class="h-4 w-4" />
			</span>
			<span class="mobile-category-tab-label">{category.name}</span>
		</a>
	{/each}
</nav>
