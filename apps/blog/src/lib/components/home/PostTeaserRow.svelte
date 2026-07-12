<script lang="ts">
	import type { Author, PillarAccent, Post } from '$lib/data';
	import { formatDispatchDate } from '$lib/utils/format-date';
	import AuthorByline from '../shared/AuthorByline.svelte';
	import PillarDot from '../shared/PillarDot.svelte';

	let { post, author, accent }: { post: Post; author: Author; accent: PillarAccent } = $props();
</script>

<a href="/articles/{post.slug}" class="block border-t border-ink/10 py-4 first:border-t-0">
	<div class="flex items-center gap-2 font-label text-xs tracking-widest text-ink/60 uppercase">
		<PillarDot {accent} />
		{formatDispatchDate(post.publishedAt)}
		<span class="text-ink/30">·</span>
		{post.readingMinutes} min
	</div>
	<h3 class="mt-1 font-display text-lg font-bold">{post.title}</h3>
	<p class="mt-1 text-sm text-ink/70">{post.excerpt}</p>
	<div class="mt-3">
		<AuthorByline {author} />
	</div>
</a>
