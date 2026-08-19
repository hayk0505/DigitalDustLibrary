<script lang="ts">
	import { fade, fly } from 'svelte/transition';
	import { X } from 'lucide-svelte';

	let { open = $bindable(false) }: { open?: boolean } = $props();

	function close() {
		open = false;
	}

	function handleKeydown(e: KeyboardEvent) {
		if (e.key === 'Escape' && open) close();
	}
</script>

<svelte:window onkeydown={handleKeydown} />

{#if open}
	<div
		class="fixed inset-0 z-40 bg-ink/40"
		onclick={close}
		role="presentation"
		transition:fade={{ duration: 200 }}
	></div>
	<div
		class="mobile-drawer fixed top-0 right-0 bottom-0 z-50 flex w-[min(320px,85vw)] flex-col overflow-y-auto bg-paper"
		role="dialog"
		aria-modal="true"
		aria-label="Site menu"
		transition:fly={{ x: 320, duration: 250, opacity: 1 }}
	>
		<div class="flex items-center justify-between border-b border-ink/10 px-5 py-4">
			<span class="font-label text-xs tracking-widest text-ink/50 uppercase">Menu</span>
			<button type="button" onclick={close} aria-label="Close menu" class="text-ink/60 hover:text-ink">
				<X class="h-5 w-5" />
			</button>
		</div>
		<nav class="flex flex-col px-5 py-4 font-label text-sm tracking-widest uppercase">
			<a href="/archive" onclick={close} class="py-2.5 hover:text-accent-red">Archive</a>
			<a href="/become-an-author" onclick={close} class="py-2.5 hover:text-accent-red">Become an author</a>
		</nav>
	</div>
{/if}
