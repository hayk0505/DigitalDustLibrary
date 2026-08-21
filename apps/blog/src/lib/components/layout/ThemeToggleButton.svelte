<script lang="ts">
	import { Sun, Moon } from 'lucide-svelte';
	import { themeState } from '$lib/state/theme.svelte';

	let {
		class: buttonClass = 'text-ink/70 hover:text-ink',
		iconClass = 'h-4 w-4'
	}: { class?: string; iconClass?: string } = $props();
</script>

<button
	type="button"
	onclick={() => themeState.toggle()}
	aria-label={themeState.current === 'dark' ? 'Switch to light theme' : 'Switch to dark theme'}
	title="Theme"
	class={buttonClass}
>
	{#if themeState.current === 'dark'}
		<Moon class={iconClass} aria-hidden="true" />
	{:else}
		<Sun class={iconClass} aria-hidden="true" />
	{/if}
</button>
