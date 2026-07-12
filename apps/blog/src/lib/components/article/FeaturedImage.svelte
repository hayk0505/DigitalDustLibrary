<script lang="ts">
	let { width = 1600, height = 900 }: { width?: number; height?: number } = $props();
</script>

<div
	class="flex aspect-video w-full items-center justify-center rounded-none border border-dashed border-ink/20"
	style="background-image: repeating-linear-gradient(135deg, rgba(28, 26, 23, 0.06) 0 10px, transparent 10px 20px);"
>
	<span class="font-label text-xs tracking-widest text-ink/40 uppercase">
		Featured image — {width}×{height}
	</span>
</div>
