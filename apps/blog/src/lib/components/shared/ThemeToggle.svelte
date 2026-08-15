<script lang="ts">
	import { getTheme, setTheme, type Theme } from '$lib/utils/theme';

	let { class: className = 'font-label tracking-widest uppercase underline hover:text-accent-red' }: { class?: string } = $props();

	let theme = $state<Theme>('light');

	$effect(() => {
		theme = getTheme();
	});

	function toggle() {
		theme = theme === 'dark' ? 'light' : 'dark';
		setTheme(theme);
	}
</script>

<button type="button" onclick={toggle} class={className}>
	{theme === 'dark' ? 'Light' : 'Dark'}
</button>
