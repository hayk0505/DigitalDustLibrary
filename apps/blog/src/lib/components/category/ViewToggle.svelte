<script lang="ts">
	let { view = $bindable('grid') }: { view?: 'grid' | 'list' } = $props();
</script>

<div class="flex items-center gap-1 font-label text-xs tracking-widest uppercase" role="group" aria-label="Article view">
	<button
		type="button"
		onclick={() => (view = 'grid')}
		aria-pressed={view === 'grid'}
		class="view-toggle-btn px-3 py-1.5 transition-colors {view === 'grid' ? 'is-active' : ''}"
	>
		Grid
	</button>
	<button
		type="button"
		onclick={() => (view = 'list')}
		aria-pressed={view === 'list'}
		class="view-toggle-btn px-3 py-1.5 transition-colors {view === 'list' ? 'is-active' : ''}"
	>
		List
	</button>
</div>
