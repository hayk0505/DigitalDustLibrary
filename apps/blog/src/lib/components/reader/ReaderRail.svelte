<script lang="ts">
	import { List, Share2, Sun, Moon } from 'lucide-svelte';
	import { getTheme, setTheme, type Theme } from '$lib/utils/theme';
	import ReaderTextSettingsPanel from './ReaderTextSettingsPanel.svelte';
	import ReaderContentsPanel from './ReaderContentsPanel.svelte';

	let { shareUrl }: { shareUrl: string } = $props();

	let theme = $state<Theme>('light');

	$effect(() => {
		theme = getTheme();
	});

	function toggleTheme() {
		theme = theme === 'dark' ? 'light' : 'dark';
		setTheme(theme);
	}

	const linkedInHref = $derived(
		`https://www.linkedin.com/sharing/share-offsite/?url=${encodeURIComponent(shareUrl)}`
	);
</script>

<!-- No mark/exit control here — that lives in ReaderHeader now, the bar
     above this rail and the article column both. -->
<div class="dd-reader-rail">
	<div class="dd-reader-rail-mid">
		<!-- dockLeft matches .dd-reader-rail's own width (84px) — the panel
		     docks flush against the rail's edge instead of floating near
		     the trigger, see anchor-panel.ts's positionDockedPanel. -->
		<ReaderContentsPanel class="dd-reader-rail-btn" dockLeft={84}>
			<List class="h-4 w-4" aria-hidden="true" />
			Contents
		</ReaderContentsPanel>

		<a
			href={linkedInHref}
			target="_blank"
			rel="noopener noreferrer"
			class="dd-reader-rail-btn"
			aria-label="Share on LinkedIn"
			title="Share"
		>
			<Share2 class="h-4 w-4" aria-hidden="true" />
			Share
		</a>

		<ReaderTextSettingsPanel class="dd-reader-rail-btn" dockLeft={84}>
			<span class="dd-reader-aa" aria-hidden="true">Aa</span>
			Text
		</ReaderTextSettingsPanel>

		<button
			type="button"
			onclick={toggleTheme}
			class="dd-reader-rail-btn"
			aria-label={theme === 'dark' ? 'Switch to light theme' : 'Switch to dark theme'}
			title="Theme"
		>
			{#if theme === 'dark'}
				<Moon class="h-4 w-4" aria-hidden="true" />
			{:else}
				<Sun class="h-4 w-4" aria-hidden="true" />
			{/if}
			{theme === 'dark' ? 'Dark' : 'Light'}
		</button>
	</div>

	<div class="dd-reader-rail-bottom">
		<div class="dd-reader-barcode" aria-hidden="true"></div>
	</div>
</div>
