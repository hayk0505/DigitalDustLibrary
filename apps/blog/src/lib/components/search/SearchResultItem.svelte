<script lang="ts">
	import type { Post } from '$lib/data';
	import CategoryDot from '../shared/CategoryDot.svelte';
	import AuthorByline from '../shared/AuthorByline.svelte';

	let {
		post,
		index,
		compact = false,
		active = false,
		onSelect
	}: {
		post: Post;
		index: number;
		compact?: boolean;
		active?: boolean;
		onSelect?: () => void;
	} = $props();
</script>

<a
	href="/articles/{post.slug}"
	onclick={onSelect}
	class="flex gap-3 border-t border-ink/10 py-3 first:border-t-0 hover:bg-ink/[0.03] {active
		? 'bg-ink/[0.05]'
		: ''}"
>
	<span class="w-6 shrink-0 font-label text-xs tabular-nums text-ink/40">
		{String(index + 1).padStart(2, '0')}
	</span>
	<div class="min-w-0 flex-1">
		<h3 class="font-display text-base font-bold text-ink">{post.title}</h3>
		<div
			class="mt-1 flex items-center gap-2 font-label text-xs tracking-widest text-ink/60 uppercase"
		>
			<CategoryDot color={post.categoryColor} />
			{post.categoryName}
			<span class="text-ink/30">·</span>
			{post.readingMinutes} min
		</div>
		{#if !compact}
			<p class="mt-1 text-sm text-ink/70">{post.excerpt}</p>
			<div class="mt-2">
				<AuthorByline author={{ name: post.authorName, handle: post.authorHandle }} />
			</div>
		{:else}
			<p class="mt-1 truncate font-label text-xs text-ink/50">{post.authorName}</p>
		{/if}
	</div>
</a>
