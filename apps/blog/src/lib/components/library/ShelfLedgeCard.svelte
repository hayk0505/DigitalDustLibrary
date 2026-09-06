<script lang="ts">
	import type { Post } from '$lib/data';
	import { getAvatarColor } from '$lib/utils/avatar-color';
	import { getCategoryTabColor, getCategoryTextColor } from '$lib/utils/category-visuals';
	import { formatDispatchDate } from '$lib/utils/format-date';
	import Avatar from '../shared/Avatar.svelte';

	let { post }: { post: Post } = $props();

	const tone = $derived(post.categoryFolderColor ?? getCategoryTabColor(post.categorySlug));
	const ink = $derived(getCategoryTextColor(tone));
</script>

<a
	href="/articles/{post.slug}"
	class="article-card block overflow-hidden rounded-card border p-3 transition-colors"
>
	<div class="flex items-center justify-between">
		<span
			class="inline-flex items-center gap-1.5 rounded-full px-2 py-0.5 font-label text-[10px] font-semibold tracking-widest uppercase"
			style="background-color: {tone}; color: {ink};"
		>
			{post.categoryName}
			<span class="opacity-70">{String(post.dispatchNumber).padStart(2, '0')}</span>
		</span>
		<span class="font-label text-xs text-ink/65">{post.readingMinutes} min</span>
	</div>

	{#if post.featuredImageUrl}
		<div class="rounded-image mt-3 aspect-[4/3] w-full overflow-hidden">
			<img src={post.featuredImageUrl} alt="" class="h-full w-full object-cover" />
		</div>
	{:else}
		<div
			class="rounded-image mt-3 flex aspect-[4/3] w-full items-center justify-center p-4 text-center"
			style="background-color: {tone}; color: {ink};"
		>
			<span class="font-display text-lg font-bold">{post.title}</span>
		</div>
	{/if}

	<h3 class="mt-3 font-display text-lg font-bold text-ink">{post.title}</h3>

	<div class="mt-3 flex items-center gap-2 border-t border-ink/10 pt-3">
		<Avatar name={post.authorName} colorClass={getAvatarColor(post.authorHandle)} size="sm" />
		<span class="truncate font-label text-xs text-ink/70">{post.authorName}</span>
		<span class="ml-auto shrink-0 font-label text-xs text-ink/65"
			>{formatDispatchDate(post.publishedAt)}</span
		>
	</div>
</a>
