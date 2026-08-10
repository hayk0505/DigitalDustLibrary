<script lang="ts">
	import '../app.css';

	let { children } = $props();
</script>

<svelte:head>
	<link rel="alternate" type="application/rss+xml" title="Digital Dust Library" href="/rss.xml" />
</svelte:head>

{@render children()}
