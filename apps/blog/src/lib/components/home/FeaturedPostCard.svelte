<script lang="ts">
	import type { Post } from '$lib/data';
	import AuthorByline from '../shared/AuthorByline.svelte';

	let { post, color }: { post: Post; color: string } = $props();
</script>

<a
	href="/articles/{post.slug}"
	class="article-card relative block overflow-hidden rounded-lg border border-ink/15 bg-paper p-6 text-ink transition-colors hover:border-ink/30"
>
	<span class="font-label text-xs tracking-widest text-ink/50 uppercase">Featured</span>
	<span
		class="accent-text pointer-events-none absolute top-4 right-6 font-display text-5xl font-bold"
		style="--accent: {color}"
	>
		{String(post.dispatchNumber).padStart(2, '0')}
	</span>
	<h3 class="mt-4 max-w-[85%] font-label text-lg font-bold">{post.title}</h3>
	<p class="mt-2 max-w-[85%] text-sm text-ink/70">{post.excerpt}</p>
	<div class="mt-6 flex items-center justify-between border-t border-ink/10 pt-4">
		<AuthorByline author={{ name: post.authorName, handle: post.authorHandle }} />
		<span class="font-label text-xs text-ink/50">{post.readingMinutes} min</span>
	</div>
</a>
