<script lang="ts">
	import type { Post } from '$lib/data';
	import ShelfLedgeCard from './ShelfLedgeCard.svelte';

	let { posts, viewAllHref }: { posts: Post[]; viewAllHref: string } = $props();

	const dotCount = $derived(Math.max(1, Math.ceil(posts.length / 4)));
</script>

<section class="mt-10">
	<div class="flex flex-wrap items-baseline justify-between gap-2">
		<div class="flex items-center gap-2 font-label text-xs tracking-widest uppercase">
			<span class="inline-block h-2.5 w-2.5 rounded-full bg-ink/70" aria-hidden="true"></span>
			<span class="text-lg tracking-normal normal-case">The Shelf</span>
			<span class="text-ink/65 normal-case"
				>Long-form writing, essays and pieces worth returning to.</span
			>
		</div>
		<a
			href={viewAllHref}
			class="font-label text-xs tracking-widest text-ink/60 uppercase hover:text-accent-red"
		>
			View all articles →
		</a>
	</div>

	<div class="mt-4">
		{#if posts.length === 0}
			<p class="py-10 text-center font-label text-xs uppercase text-ink/40">
				— No dispatches on this shelf yet —
			</p>
		{:else}
			<div class="grid grid-cols-2 gap-3 sm:gap-4 lg:grid-cols-4">
				{#each posts as post (post.slug)}
					<ShelfLedgeCard {post} />
				{/each}
			</div>
			{#if posts.length > 4}
				<div class="mt-4 flex items-center justify-center gap-1.5" aria-hidden="true">
					{#each { length: dotCount } as _, i (i)}
						<span class="h-1.5 w-1.5 rounded-full {i === 0 ? 'bg-ink/60' : 'bg-ink/20'}"></span>
					{/each}
				</div>
			{/if}
		{/if}
	</div>
</section>
