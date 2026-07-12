<script lang="ts">
	import SiteHeader from '$lib/components/layout/SiteHeader.svelte';

	let { children } = $props();
</script>

<SiteHeader />

<main class="mx-auto max-w-6xl px-6 py-8">
	{@render children()}
</main>
