<script lang="ts">
	import { Sun, Moon, X } from 'lucide-svelte';
	import { readerMode } from '$lib/state/reader-mode.svelte';
	import { getTheme, setTheme, type Theme } from '$lib/utils/theme';
	import ReaderTextSettingsPanel from './ReaderTextSettingsPanel.svelte';
	import logo from '$lib/assets/logo.svg';

	let theme = $state<Theme>('light');

	$effect(() => {
		theme = getTheme();
	});

	function toggleTheme() {
		theme = theme === 'dark' ? 'light' : 'dark';
		setTheme(theme);
	}
</script>

<div class="dd-reader-topbar">
	<!-- Static, not a control — matches ReaderHeader's desktop brand mark,
	     which also isn't clickable. Exiting has its own dedicated button
	     below now rather than overloading the mark with that job. -->
	<img src={logo} alt="Digital Dust Library" class="dd-reader-mark" />

	<div class="dd-reader-topbar-actions">
		<ReaderTextSettingsPanel class="dd-reader-topbar-btn dd-reader-aa">Aa</ReaderTextSettingsPanel>

		<button
			type="button"
			onclick={toggleTheme}
			class="dd-reader-topbar-btn"
			aria-label={theme === 'dark' ? 'Switch to light theme' : 'Switch to dark theme'}
			title="Theme"
		>
			{#if theme === 'dark'}
				<Moon class="h-4 w-4" aria-hidden="true" />
			{:else}
				<Sun class="h-4 w-4" aria-hidden="true" />
			{/if}
		</button>

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
