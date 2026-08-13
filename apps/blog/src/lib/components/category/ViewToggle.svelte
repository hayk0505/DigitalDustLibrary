<script lang="ts">
	let { view = $bindable('grid') }: { view?: 'grid' | 'list' } = $props();
</script>

<div class="flex items-center gap-1 font-label text-xs tracking-widest uppercase">
	<button
		type="button"
		onclick={() => (view = 'grid')}
		class="border px-3 py-1.5 {view === 'grid'
			? 'border-ink bg-ink text-paper'
			: 'border-ink/20 text-ink/60 hover:border-ink/40'}"
	>
		Grid
	</button>
	<button
		type="button"
		onclick={() => (view = 'list')}
		class="border px-3 py-1.5 {view === 'list'
			? 'border-ink bg-ink text-paper'
			: 'border-ink/20 text-ink/60 hover:border-ink/40'}"
	>
		List
	</button>
</div>
