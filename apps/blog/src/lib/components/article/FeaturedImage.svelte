<script lang="ts">
	let {
		url = null,
		width = 1600,
		height = 900
	}: { url?: string | null; width?: number; height?: number } = $props();
</script>

{#if url}
	<img src={url} alt="" class="aspect-video w-full rounded-none object-cover" />
{:else}
	<div
		class="flex aspect-video w-full items-center justify-center rounded-none border border-dashed border-ink/20"
		style="background-image: repeating-linear-gradient(135deg, rgba(28, 26, 23, 0.06) 0 10px, transparent 10px 20px);"
	>
		<span class="font-label text-xs tracking-widest text-ink/40 uppercase">
			Featured image — {width}×{height}
		</span>
	</div>
{/if}
