<script lang="ts">
	import ArticleBody from '$lib/components/article/ArticleBody.svelte';
	import ArticleMeta from '$lib/components/article/ArticleMeta.svelte';
	import ArticleTopBar from '$lib/components/article/ArticleTopBar.svelte';
	import FeaturedImage from '$lib/components/article/FeaturedImage.svelte';
	import RelatedArticles from '$lib/components/article/RelatedArticles.svelte';
	import ReaderHeader from '$lib/components/reader/ReaderHeader.svelte';
	import ReaderRail from '$lib/components/reader/ReaderRail.svelte';
	import ReaderTopBar from '$lib/components/reader/ReaderTopBar.svelte';
	import ReaderNavBarDesktop from '$lib/components/reader/ReaderNavBarDesktop.svelte';
	import ReaderNavBarMobile from '$lib/components/reader/ReaderNavBarMobile.svelte';
	import SeoHead from '$lib/components/shared/SeoHead.svelte';
	import { readerMode } from '$lib/state/reader-mode.svelte';
	import { formatDispatchDate } from '$lib/utils/format-date';
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

{#if readerMode.active}
	<!-- A standalone chrome, not a themed variant of the normal article page
	     — see $lib/state/reader-mode.svelte.ts and app.css's "Reader Mode"
	     section. ArticleBody (and its existing first-paragraph drop cap) is
	     the one piece reused unchanged from the normal view below; text
	     size/line-height/width are applied here via CSS custom properties
	     that .dd-reader-article's rules read, rather than new props on
	     ArticleBody itself. -->
	<div class="dd-reader-mode">
		<ReaderTopBar />
		<ReaderHeader title={data.post.title} />
		<div class="flex">
			<ReaderRail shareUrl={page.url.href} />
			<div class="min-w-0 flex-1">
				<article
					class="dd-reader-article"
					style="--reader-width: {readerMode.readingWidth}px; --reader-font-size: {readerMode.fontSize}px; --reader-line-height: {readerMode.lineHeight};"
				>
					<h1 class="dd-reader-title">{data.post.title}</h1>
					<div class="dd-reader-divider" aria-hidden="true"></div>
					<p class="dd-reader-meta">
						{data.post.categoryName} · {formatDispatchDate(data.post.publishedAt)} · {data.post
							.readingMinutes} min read
					</p>
					<ArticleBody html={data.post.bodyHtml} />
				</article>
			</div>
		</div>
		<ReaderNavBarDesktop prevPost={data.prevPost} nextPost={data.nextPost} />
		<ReaderNavBarMobile shareUrl={page.url.href} prevPost={data.prevPost} nextPost={data.nextPost} />
	</div>
{:else}
	<ArticleTopBar shareUrl={page.url.href} />

	<article class="mx-auto max-w-3xl  py-10">
		<ArticleMeta post={data.post} />
		<div class="mt-8">
			<FeaturedImage url={data.post.featuredImageUrl} />
		</div>
		<ArticleBody html={data.post.bodyHtml} />
	</article>

	<!-- Not shown in Reader Mode — see the branch above and app.css's Reader
	     Mode section, which is deliberately a quiet, standalone reading
	     surface with everything but the article itself stripped away.
	     No px-6 here — <main> in (site)/+layout.svelte already supplies
	     the page's horizontal padding, same as <article> above relies on. -->
	<div class="mx-auto max-w-3xl pb-10">
		<RelatedArticles posts={data.relatedPosts} />
	</div>
{/if}
