<script lang="ts">
	import type { Post } from '$lib/data';
	import {
		getCategoryIcon,
		getCategoryTabColor,
		getCategoryTextColor
	} from '$lib/utils/category-visuals';
	import CategoryDot from '../shared/CategoryDot.svelte';

	let { post }: { post: Post } = $props();

	const tone = $derived(post.categoryFolderColor ?? getCategoryTabColor(post.categorySlug));
	const ink = $derived(getCategoryTextColor(tone));
	const Icon = $derived(getCategoryIcon(post.categorySlug));
</script>

<a
	href="/articles/{post.slug}"
	class="group flex gap-3 border-t border-ink/10 p-4 transition-colors first:border-t-0 hover:bg-ink/[0.03]"
>
	{#if post.featuredImageUrl}
		<div class="h-20 w-20 shrink-0 overflow-hidden rounded-image">
			<img src={post.featuredImageUrl} alt="" class="h-full w-full object-cover" />
		</div>
	{:else}
		<div
			class="flex h-20 w-20 shrink-0 items-center justify-center rounded-image"
			style="background-color: {tone}; color: {ink};"
		>
			<Icon class="h-6 w-6" />
		</div>
	{/if}
	<div class="flex min-w-0 flex-1 flex-col gap-1.5">
		<div class="flex items-center gap-2 font-label text-xs tracking-widest text-ink/60 uppercase">
			<CategoryDot color={tone} />
			{post.categoryName}
			<span class="text-ink/30">·</span>
			<span class="font-display text-base font-bold text-ink normal-case transition-colors"
				>{post.title}</span
			>
		</div>
		<p class="text-sm text-ink/70">{post.excerpt}</p>
		<div class="mt-1 flex items-center justify-between gap-2 font-label text-xs text-ink/50">
			<span>{post.authorName}</span>
			<span>{post.readingMinutes} min</span>
		</div>
	</div>
</a>
