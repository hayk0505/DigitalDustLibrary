<script lang="ts">
	let { baseHref, active }: { baseHref: string; active: 'all' | 'mini' | 'shelf' } = $props();
</script>

<div
	class="mb-4 flex items-center gap-1 font-label text-xs tracking-widest uppercase"
	role="group"
	aria-label="Filter by format"
>
	<a
		href={baseHref}
		aria-current={active === 'all' ? 'page' : undefined}
		class="format-toggle-btn px-3 py-1.5 transition-colors {active === 'all' ? 'is-active' : ''}"
	>
		All
	</a>
	<a
		href="{baseHref}?type=mini"
		aria-current={active === 'mini' ? 'page' : undefined}
		class="format-toggle-btn px-3 py-1.5 transition-colors {active === 'mini' ? 'is-active' : ''}"
	>
		Minis
	</a>
	<a
		href="{baseHref}?type=shelf"
		aria-current={active === 'shelf' ? 'page' : undefined}
		class="format-toggle-btn px-3 py-1.5 transition-colors {active === 'shelf' ? 'is-active' : ''}"
	>
		Shelf
	</a>
</div>
