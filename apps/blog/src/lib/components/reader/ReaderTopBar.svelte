<script lang="ts">
	import { X } from 'lucide-svelte';
	import { readerMode } from '$lib/state/reader-mode.svelte';
	import ThemeToggleButton from '$lib/components/layout/ThemeToggleButton.svelte';
	import ReaderTextSettingsPanel from './ReaderTextSettingsPanel.svelte';
	import logo from '$lib/assets/logo.svg';
</script>

<div class="dd-reader-topbar">
	<!-- Static, not a control — matches ReaderHeader's desktop brand mark,
	     which also isn't clickable. Exiting has its own dedicated button
	     below now rather than overloading the mark with that job. -->
	<img src={logo} alt="Digital Dust Library" class="dd-reader-mark" />

	<div class="dd-reader-topbar-actions">
		<ReaderTextSettingsPanel class="dd-reader-topbar-btn dd-reader-aa">Aa</ReaderTextSettingsPanel>

		<ThemeToggleButton class="dd-reader-topbar-btn" />

		<button
			type="button"
			onclick={() => readerMode.exit()}
			class="dd-reader-topbar-btn"
			aria-label="Exit Reader Mode"
			title="Exit Reader Mode"
		>
			<X class="h-4 w-4" aria-hidden="true" />
		</button>
	</div>
</div>
