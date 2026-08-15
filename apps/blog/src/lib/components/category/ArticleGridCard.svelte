<script lang="ts">
	import type { Post } from '$lib/data';
	import { formatDispatchDate } from '$lib/utils/format-date';
	import { getAvatarColor } from '$lib/utils/avatar-color';
	import CategoryDot from '../shared/CategoryDot.svelte';
	import Avatar from '../shared/Avatar.svelte';

	let { post, color }: { post: Post; color: string } = $props();
</script>

<a
	href="/articles/{post.slug}"
	class="article-card block overflow-hidden rounded-card border p-3 transition-colors"
>
	<div class="flex items-center gap-2 font-label text-[11px] tracking-widest text-ink/60 uppercase">
		<CategoryDot {color} />
		{formatDispatchDate(post.publishedAt)}
		<span class="text-ink/30">·</span>
		{post.readingMinutes} min
	</div>

	{#if post.featuredImageUrl}
		<div class="rounded-image mt-3 aspect-[4/3] w-full overflow-hidden">
			<img src={post.featuredImageUrl} alt="" class="h-full w-full object-cover" />
		</div>
	{:else}
		<div class="no-image-placeholder rounded-image mt-3 flex aspect-[4/3] w-full items-center justify-center">
			<span class="font-label text-[10px] tracking-widest text-ink/40 uppercase">No image</span>
		</div>
	{/if}

	<h3 class="mt-3 font-display text-lg font-bold text-ink">{post.title}</h3>

	<div class="mt-3 flex items-center justify-between gap-2 border-t border-ink/10 pt-3">
		<div class="flex min-w-0 items-center gap-2">
			<Avatar name={post.authorName} colorClass={getAvatarColor(post.authorHandle)} size="sm" />
			<span class="truncate font-label text-xs text-ink/70">{post.authorName}</span>
		</div>
		<span class="shrink-0 font-label text-xs text-ink/50">{post.readingMinutes} min</span>
	</div>
</a>
