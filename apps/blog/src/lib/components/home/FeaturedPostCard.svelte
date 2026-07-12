<script lang="ts">
	import { pillarAccentClasses, type Author, type PillarAccent, type Post } from '$lib/data';
	import AuthorByline from '../shared/AuthorByline.svelte';

	let { post, author, accent }: { post: Post; author: Author; accent: PillarAccent } = $props();
</script>

<a
	href="/articles/{post.slug}"
	class="relative block overflow-hidden rounded-none bg-ink p-6 text-paper transition-colors hover:bg-ink/90"
>
	<span class="font-label text-xs tracking-widest text-paper/60 uppercase">Featured</span>
	<span
		class="pointer-events-none absolute top-2 right-4 font-display text-7xl font-bold {pillarAccentClasses[
			accent
		].text}"
	>
		{String(post.dispatchNumber).padStart(2, '0')}
	</span>
	<h3 class="mt-2 max-w-[80%] font-display text-xl font-bold">{post.title}</h3>
	<p class="mt-2 text-sm text-paper/80">{post.excerpt}</p>
	<div class="mt-4 flex items-center justify-between border-t border-paper/20 pt-4">
		<AuthorByline {author} />
		<span class="font-label text-xs text-paper/60">{post.readingMinutes} min</span>
	</div>
</a>
