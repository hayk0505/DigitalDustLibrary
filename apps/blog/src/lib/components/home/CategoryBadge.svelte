<script lang="ts">
	import type { Category } from '$lib/data';
	import { getCategoryTabColor } from '$lib/utils/category-visuals';
	import CategoryDot from '../shared/CategoryDot.svelte';

	let { category, postCount }: { category: Category; postCount: number } = $props();

	const accentColor = $derived(category.folderColor ?? getCategoryTabColor(category.slug));
</script>

<header class="mb-4">
	<div class="flex items-center justify-between font-label text-xs tracking-widest uppercase">
		<span class="flex items-center gap-2">
			<CategoryDot color={accentColor} size="md" />
			Column {String(category.position).padStart(2, '0')}
		</span>
		<span class="text-ink/50">{postCount} posts</span>
	</div>
	<h2 class="mt-1 font-label text-2xl uppercase">
		<a href="/category/{category.slug}" class="hover:text-accent-red">{category.name}</a>
	</h2>
	<p class="mt-1 text-sm text-ink/60">{category.description}</p>
	<div class="accent-bg mt-2 h-1 w-10" style="--accent: {accentColor}"></div>
</header>
