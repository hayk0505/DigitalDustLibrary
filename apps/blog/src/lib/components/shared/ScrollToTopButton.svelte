<script lang="ts">
	import { fade } from 'svelte/transition';
	import { ArrowUp } from 'lucide-svelte';

	let visible = $state(false);

	function handleScroll() {
		visible = window.scrollY > 400;
	}

	function scrollToTop() {
		window.scrollTo({ top: 0, behavior: 'smooth' });
	}
</script>

<svelte:window onscroll={handleScroll} />

{#if visible}
	<button
		type="button"
		onclick={scrollToTop}
		aria-label="Scroll to top"
		class="scroll-top-btn fixed right-5 bottom-5 z-30 flex h-10 w-10 items-center justify-center rounded-full"
		transition:fade={{ duration: 150 }}
	>
		<ArrowUp class="h-4 w-4" />
	</button>
{/if}
