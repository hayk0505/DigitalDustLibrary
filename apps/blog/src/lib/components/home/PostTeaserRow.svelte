<script lang="ts">
	import type { Post } from '$lib/data';
	import { formatDispatchDate } from '$lib/utils/format-date';
	import AuthorByline from '../shared/AuthorByline.svelte';
	import CategoryDot from '../shared/CategoryDot.svelte';

	let {
		post,
		color,
		showImage = false
	}: { post: Post; color: string; showImage?: boolean } = $props();
</script>

<a
	href="/articles/{post.slug}"
	class="group flex items-center justify-between gap-4 border-t border-ink/10 p-4 transition-colors first:border-t-0 hover:bg-ink/[0.03]"
>
	{#if showImage}
		<div class="list-thumb h-24 w-32 shrink-0">
			{#if post.featuredImageUrl}
				<img src={post.featuredImageUrl} alt="" class="h-full w-full object-cover" />
			{:else}
				<div class="no-image-placeholder flex h-full w-full items-center justify-center">
					<span class="font-label text-[10px] tracking-widest text-ink/40 uppercase">No image</span>
				</div>
			{/if}
		</div>
	{/if}
	<div class="min-w-0 flex-1">
		<div class="flex items-center gap-2 font-label text-xs tracking-widest text-ink/60 uppercase">
			<CategoryDot {color} />
			{formatDispatchDate(post.publishedAt)}
			<span class="text-ink/30">·</span>
			{post.readingMinutes} min
		</div>
		<h3
			class="mt-1 font-display text-lg font-bold transition-colors"
			style="--hover-color: {color}"
		>
			{post.title}
		</h3>
		<p class="mt-1 text-sm text-ink/70">{post.excerpt}</p>
		<div class="mt-3 flex items-center justify-between gap-2">
			<AuthorByline author={{ name: post.authorName, handle: post.authorHandle }} />
			<span class="shrink-0 font-label text-xs text-ink/50">{post.readingMinutes} min</span>
		</div>
	</div>
</a>
