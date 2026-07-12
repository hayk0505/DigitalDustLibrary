<script lang="ts">
	import ArticleBody from '$lib/components/article/ArticleBody.svelte';
	import ArticleMeta from '$lib/components/article/ArticleMeta.svelte';
	import ArticleTopBar from '$lib/components/article/ArticleTopBar.svelte';
	import FeaturedImage from '$lib/components/article/FeaturedImage.svelte';
	import { page } from '$app/state';
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();
</script>

<svelte:head>
	<title>{data.post.title} — Digital Dust Library</title>
	<meta name="description" content={data.post.excerpt} />
</svelte:head>

<ArticleTopBar shareUrl={page.url.href} />

<article class="mx-auto max-w-3xl px-6 py-10">
	<ArticleMeta post={data.post} author={data.author} />
	<div class="mt-8">
		<FeaturedImage />
	</div>
	<ArticleBody html={data.post.body} />
</article>
