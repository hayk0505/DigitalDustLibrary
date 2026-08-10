<script lang="ts">
	import { pillarAccentClasses, getPillarBySlug, type Post } from '$lib/data';
	import { formatDispatchDate } from '$lib/utils/format-date';
	import AuthorByline from '$lib/components/shared/AuthorByline.svelte';
	import PillarDot from '$lib/components/shared/PillarDot.svelte';

	let { post }: { post: Post } = $props();

	const pillar = $derived(getPillarBySlug(post.pillarSlug));
</script>

{#if pillar}
	<div class="flex items-center gap-2 font-label text-xs tracking-widest uppercase">
		<PillarDot accent={pillar.accent} size="md" />
		<span class={pillarAccentClasses[pillar.accent].text}>{pillar.label}</span>
		<span class="text-ink/30">/</span>
		<span class="text-ink/60">{formatDispatchDate(post.publishedAt)}</span>
		<span class="text-ink/30">/</span>
		<span class="text-ink/60">{post.readingMinutes} min</span>
	</div>
{/if}

<h1 class="mt-3 font-display text-4xl font-bold">{post.title}</h1>
<p class="mt-3 font-display text-lg text-ink/70 italic">{post.excerpt}</p>

<div class="mt-6 border-t border-ink/10 pt-6">
	<AuthorByline author={{ name: post.authorName, handle: post.authorHandle }} />
</div>
