<script lang="ts">
	import { pillarAccentClasses, type Author, type PillarAccent, type Post } from '$lib/data';
	import { formatDispatchDate } from '$lib/utils/format-date';
	import AuthorByline from '../shared/AuthorByline.svelte';
	import PillarDot from '../shared/PillarDot.svelte';

	let { post, author, accent }: { post: Post; author: Author; accent: PillarAccent } = $props();
</script>

<a
	href="/articles/{post.slug}"
	class="group block border-t border-ink/10 py-4 transition-colors first:border-t-0 hover:bg-ink/[0.03]"
>
	<div class="flex items-center gap-2 font-label text-xs tracking-widest text-ink/60 uppercase">
		<PillarDot {accent} />
		{formatDispatchDate(post.publishedAt)}
		<span class="text-ink/30">·</span>
		{post.readingMinutes} min
	</div>
	<h3
		class="mt-1 font-display text-lg font-bold transition-colors {pillarAccentClasses[accent]
			.groupHoverText}"
	>
		{post.title}
	</h3>
	<p class="mt-1 text-sm text-ink/70">{post.excerpt}</p>
	<div class="mt-3">
		<AuthorByline {author} />
	</div>
</a>
