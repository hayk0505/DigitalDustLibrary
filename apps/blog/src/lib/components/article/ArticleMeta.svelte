<script lang="ts">
	import { formatDispatchDate } from '$lib/utils/format-date';
	import { getCategoryTabColor } from '$lib/utils/category-visuals';
	import AuthorByline from '$lib/components/shared/AuthorByline.svelte';
	import CategoryDot from '$lib/components/shared/CategoryDot.svelte';
	import type { Post } from '$lib/data';

	let { post }: { post: Post } = $props();

	const accentColor = $derived(post.categoryFolderColor ?? getCategoryTabColor(post.categorySlug));
</script>

<div class="flex items-center gap-2 font-label text-xs tracking-widest uppercase">
	<CategoryDot color={accentColor} size="md" />
	<span class="accent-text" style="--accent: {accentColor}">{post.categoryName}</span>
	<span class="text-ink/30">/</span>
	<span class="text-ink/60">{formatDispatchDate(post.publishedAt)}</span>
	<span class="text-ink/30">/</span>
	<span class="text-ink/60">{post.readingMinutes} min</span>
</div>

<h1 class="mt-3 font-display text-4xl font-bold">{post.title}</h1>
<p class="mt-3 font-display text-lg text-ink/70 italic">{post.excerpt}</p>

<div class="mt-6 border-t border-ink/10 pt-6">
	<AuthorByline author={{ name: post.authorName, handle: post.authorHandle }} />
</div>
