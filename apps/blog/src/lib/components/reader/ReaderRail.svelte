<script lang="ts">
	import { List, Share2, Sun, Moon } from 'lucide-svelte';
	import { themeState } from '$lib/state/theme.svelte';
	import ReaderTextSettingsPanel from './ReaderTextSettingsPanel.svelte';
	import ReaderContentsPanel from './ReaderContentsPanel.svelte';

	let { shareUrl }: { shareUrl: string } = $props();

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
			onclick={() => themeState.toggle()}
			class="dd-reader-rail-btn"
			aria-label={themeState.current === 'dark' ? 'Switch to light theme' : 'Switch to dark theme'}
			title="Theme"
		>
			{#if themeState.current === 'dark'}
				<Moon class="h-4 w-4" aria-hidden="true" />
			{:else}
				<Sun class="h-4 w-4" aria-hidden="true" />
			{/if}
			{themeState.current === 'dark' ? 'Dark' : 'Light'}
		</button>
	</div>

	<div class="dd-reader-rail-bottom">
		<div class="dd-reader-barcode" aria-hidden="true"></div>
	</div>
</div>
