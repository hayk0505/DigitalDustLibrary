<script lang="ts">
	import type { Post } from '$lib/data';
	import { getCategoryTabColor } from '$lib/utils/category-visuals';
	import CategoryDot from '../shared/CategoryDot.svelte';

	let { post }: { post: Post } = $props();

	const tone = $derived(post.categoryFolderColor ?? getCategoryTabColor(post.categorySlug));
</script>

<a
	href="/articles/{post.slug}"
	class="group flex flex-col gap-1.5 border-t border-ink/10 p-4 transition-colors first:border-t-0 hover:bg-ink/[0.03]"
>
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
</a>
