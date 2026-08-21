<script lang="ts">
	import { Menu } from 'lucide-svelte';
	import MobileMenuDrawer from './MobileMenuDrawer.svelte';

	let {
		class: buttonClass = 'text-ink/70 hover:text-ink',
		iconClass = 'h-5 w-5'
	}: { class?: string; iconClass?: string } = $props();

	let open = $state(false);
</script>

<button
	type="button"
	onclick={() => (open = true)}
	aria-label="Open menu"
	class={buttonClass}
>
	<Menu class={iconClass} />
</button>

<MobileMenuDrawer bind:open />
