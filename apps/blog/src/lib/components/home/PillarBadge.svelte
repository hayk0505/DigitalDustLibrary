<script lang="ts">
	import { pillarAccentClasses, type Pillar } from '$lib/data';
	import PillarDot from '../shared/PillarDot.svelte';

	let { pillar, postCount }: { pillar: Pillar; postCount: number } = $props();
</script>

<header class="mb-4">
	<div class="flex items-center justify-between font-label text-xs tracking-widest uppercase">
		<span class="flex items-center gap-2">
			<PillarDot accent={pillar.accent} size="md" />
			Pillar 0{pillar.index}
		</span>
		<span class="text-ink/50">{postCount} posts</span>
	</div>
	<h2 class="mt-1 font-display text-2xl font-bold uppercase">{pillar.label}</h2>
	<div class="mt-2 h-1 w-10 {pillarAccentClasses[pillar.accent].bg}"></div>
</header>
