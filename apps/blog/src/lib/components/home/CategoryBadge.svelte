<script lang="ts">
	import type { Category } from '$lib/data';
	import CategoryDot from '../shared/CategoryDot.svelte';

	let { category, postCount }: { category: Category; postCount: number } = $props();
</script>

<header class="mb-4">
	<div class="flex items-center justify-between font-label text-xs tracking-widest uppercase">
		<span class="flex items-center gap-2">
			<CategoryDot color={category.color} size="md" />
			Column {String(category.position).padStart(2, '0')}
		</span>
		<span class="text-ink/50">{postCount} posts</span>
	</div>
	<h2 class="mt-1 font-display text-2xl font-bold uppercase">{category.name}</h2>
	<p class="mt-1 text-sm text-ink/60">{category.description}</p>
	<div class="mt-2 h-1 w-10" style="background-color: {category.color}"></div>
</header>
