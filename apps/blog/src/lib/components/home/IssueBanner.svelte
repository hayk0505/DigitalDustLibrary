<script lang="ts">
	let {
		volumeLabel,
		dispatchCount,
		showPager = false,
		catPage = 0,
		totalCatPages = 1,
		onPrev,
		onNext
	}: {
		volumeLabel: string;
		dispatchCount: number;
		showPager?: boolean;
		catPage?: number;
		totalCatPages?: number;
		onPrev?: () => void;
		onNext?: () => void;
	} = $props();
</script>

<div
	class="flex flex-col gap-1 border-b border-ink/10 pb-3 font-label text-xs tracking-widest text-ink/60 uppercase sm:flex-row sm:items-center sm:justify-between"
>
	<span>{volumeLabel}</span>
	<span class="flex items-center gap-3">
		{dispatchCount} dispatches · Scroll each column ↓
		{#if showPager}
			<span class="flex items-center gap-1">
				<button
					type="button"
					disabled={catPage === 0}
					onclick={onPrev}
					class="disabled:opacity-30"
					aria-label="Previous categories"
				>
					←
				</button>
				<span>{catPage + 1}/{totalCatPages}</span>
				<button
					type="button"
					disabled={catPage >= totalCatPages - 1}
					onclick={onNext}
					class="disabled:opacity-30"
					aria-label="Next categories"
				>
					→
				</button>
			</span>
		{/if}
	</span>
</div>
