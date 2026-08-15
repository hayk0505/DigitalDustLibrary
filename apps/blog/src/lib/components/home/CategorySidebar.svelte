<script lang="ts">
	import type { Category } from '$lib/data';
	import CategoryRail from './CategoryRail.svelte';

	let { categories }: { categories: Category[] } = $props();
</script>

<div class="hidden shrink-0 self-start md:sticky md:top-0 md:flex md:h-screen">
	<CategoryRail {categories} />
</div>
