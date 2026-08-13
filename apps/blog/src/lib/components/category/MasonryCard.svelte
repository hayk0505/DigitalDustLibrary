<script lang="ts">
	import type { Post } from '$lib/data';
	import { getCardHeight, isTallCard } from '$lib/utils/masonry-height';

	let { post, color }: { post: Post; color: string } = $props();

	const height = $derived(getCardHeight(post.slug));
	const tall = $derived(isTallCard(post.slug));
</script>

<a href="/articles/{post.slug}" class="group mb-4 block break-inside-avoid">
	<div class="relative w-full overflow-hidden" style="height: {height}px">
		{#if post.featuredImageUrl}
			<img src={post.featuredImageUrl} alt="" class="h-full w-full object-cover" />
		{:else}
			<div
				class="flex h-full w-full items-center justify-center border border-dashed border-ink/20"
				style="background-image: repeating-linear-gradient(135deg, rgba(28, 26, 23, 0.06) 0 10px, transparent 10px 20px);"
			>
				<span class="font-label text-xs tracking-widest text-ink/40 uppercase">No image</span>
			</div>
		{/if}
		{#if tall}
			<div
				class="absolute inset-x-0 bottom-0 p-3 pt-10 backdrop-blur-sm"
				style="background: linear-gradient(to top, rgba(28, 26, 23, 0.85), transparent)"
			>
				<p class="line-clamp-3 text-sm text-paper">{post.excerpt}</p>
			</div>
		{/if}
	</div>
	<h3
		class="mt-2 font-display text-sm font-bold transition-colors"
		style="--hover-color: {color}"
	>
		{post.title}
	</h3>
</a>
