<script lang="ts">
	import '../app.css';

	let { children, data } = $props();
</script>

<svelte:head>
	<link
		rel="alternate"
		type="application/rss+xml"
		title={data.settings.siteTitle}
		href="/rss.xml"
	/>
</svelte:head>

{@render children()}
