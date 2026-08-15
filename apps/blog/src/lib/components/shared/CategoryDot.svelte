<script lang="ts">
	let { color, size = 'sm' }: { color: string; size?: 'sm' | 'md' } = $props();

	const sizeClass = $derived(size === 'md' ? 'h-2.5 w-2.5' : 'h-1.5 w-1.5');
</script>

<span class="accent-bg inline-block rounded-full {sizeClass}" style="--accent: {color}"></span>
