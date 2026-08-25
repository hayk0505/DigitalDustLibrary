<script lang="ts">
	import { page } from '$app/state';

	let {
		title,
		description,
		url,
		image,
		type = 'website',
		jsonLd,
		appendSiteName = true
	}: {
		title: string;
		description?: string;
		url: string;
		image?: string;
		type?: 'website' | 'article';
		jsonLd?: object;

		appendSiteName?: boolean;
	} = $props();

	const settings = $derived(page.data.settings);
	const fullTitle = $derived(appendSiteName ? `${title} — ${settings.siteTitle}` : title);
	const effectiveDescription = $derived(description ?? settings.defaultMetaDescription);
</script>

<svelte:head>
	<title>{fullTitle}</title>
	<meta name="description" content={effectiveDescription} />
	<link rel="canonical" href={url} />

	<meta property="og:title" content={fullTitle} />
	<meta property="og:description" content={effectiveDescription} />
	<meta property="og:type" content={type} />
	<meta property="og:url" content={url} />
	<meta property="og:site_name" content={settings.siteTitle} />
	{#if image}
		<meta property="og:image" content={image} />
	{/if}

	<meta name="twitter:card" content={image ? 'summary_large_image' : 'summary'} />
	<meta name="twitter:title" content={fullTitle} />
	<meta name="twitter:description" content={effectiveDescription} />
	{#if image}
		<meta name="twitter:image" content={image} />
	{/if}

	{#if jsonLd}
		{@html `<script type="application/ld+json">${JSON.stringify(jsonLd).replace(/</g, '\\u003c')}<\/script>`}
	{/if}
</svelte:head>
