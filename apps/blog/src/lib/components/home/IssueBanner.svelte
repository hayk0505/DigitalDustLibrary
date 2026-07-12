<script lang="ts">
	let { volumeLabel, dispatchCount }: { volumeLabel: string; dispatchCount: number } = $props();
</script>

<div
	class="flex flex-col gap-1 border-b border-ink/10 pb-3 font-label text-xs tracking-widest text-ink/60 uppercase sm:flex-row sm:items-center sm:justify-between"
>
	<span>{volumeLabel}</span>
	<span>Three pillars · {dispatchCount} dispatches · Scroll each column ↓</span>
</div>
