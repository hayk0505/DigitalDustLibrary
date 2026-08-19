<script lang="ts">
	import { X } from 'lucide-svelte';
	import { readerMode } from '$lib/state/reader-mode.svelte';
	import logo from '$lib/assets/logo.svg';

	let { title }: { title: string } = $props();
</script>

<div class="dd-reader-header">
	<div class="dd-reader-header-brand">
		<img src={logo} alt="" class="dd-reader-header-mark" />
		<span class="dd-reader-header-label">Reader Mode</span>
	</div>

	<p class="dd-reader-header-title">{title}</p>

	<button
		type="button"
		onclick={() => readerMode.exit()}
		class="dd-reader-header-exit"
		aria-label="Exit Reader Mode"
	>
		Exit Reader
		<X class="h-3.5 w-3.5" aria-hidden="true" />
	</button>
</div>
