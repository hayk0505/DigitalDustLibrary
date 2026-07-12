<script lang="ts">
	import { pillarAccentClasses, type PillarAccent } from '$lib/data';

	let { accent, size = 'sm' }: { accent: PillarAccent; size?: 'sm' | 'md' } = $props();

	const sizeClass = $derived(size === 'md' ? 'h-2.5 w-2.5' : 'h-1.5 w-1.5');
</script>

<span class="inline-block rounded-full {pillarAccentClasses[accent].dot} {sizeClass}"></span>
