<script lang="ts">
	import ArticleBody from '$lib/components/article/ArticleBody.svelte';
	import ArticleMeta from '$lib/components/article/ArticleMeta.svelte';
	import ArticleTopBar from '$lib/components/article/ArticleTopBar.svelte';
	import FeaturedImage from '$lib/components/article/FeaturedImage.svelte';
	import SeoHead from '$lib/components/shared/SeoHead.svelte';
	import { page } from '$app/state';
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();

	const jsonLd = $derived({
		'@context': 'https://schema.org',
		'@type': 'BlogPosting',
		headline: data.post.title,
		description: data.post.excerpt,
		...(data.post.featuredImageUrl ? { image: data.post.featuredImageUrl } : {}),
		datePublished: data.post.publishedAt,
		author: { '@type': 'Person', name: data.post.authorName },
		publisher: { '@type': 'Organization', name: 'Digital Dust Library' },
		mainEntityOfPage: { '@type': 'WebPage', '@id': page.url.href }
	});
</script>

<SeoHead
	title="{data.post.title} — Digital Dust Library"
	description={data.post.excerpt}
	url={page.url.href}
	image={data.post.featuredImageUrl ?? undefined}
	type="article"
	{jsonLd}
/>

<ArticleTopBar shareUrl={page.url.href} />

<article class="mx-auto max-w-3xl px-6 py-10">
	<ArticleMeta post={data.post} />
	<div class="mt-8">
		<FeaturedImage url={data.post.featuredImageUrl} />
	</div>
	<ArticleBody html={data.post.bodyHtml} />
</article>
