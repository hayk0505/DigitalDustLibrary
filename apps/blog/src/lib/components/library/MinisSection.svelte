<script lang="ts">
	import type { Post } from '$lib/data';
	import MiniListRow from './MiniListRow.svelte';

	let { posts, viewAllHref }: { posts: Post[]; viewAllHref: string } = $props();
</script>

<section>
	<div class="flex flex-wrap items-baseline justify-between gap-2">
		<div class="flex items-center gap-2 font-label text-xs tracking-widest uppercase">
			<span class="inline-block h-2.5 w-2.5 rounded-full bg-ink/70" aria-hidden="true"></span>
			<span class="text-lg tracking-normal normal-case">Minis</span>
			<span class="text-ink/65 normal-case"
				>Short notes, observations, experiments and things worth keeping.</span
			>
		</div>
		<a
			href={viewAllHref}
			class="font-label text-xs tracking-widest text-ink/60 uppercase hover:text-accent-red"
		>
			View all minis →
		</a>
	</div>

	<div class="mt-4">
		{#if posts.length === 0}
			<p class="py-10 text-center font-label text-xs uppercase text-ink/40">
				— No minis here yet —
			</p>
		{:else}
			{#each posts as post (post.slug)}
				<MiniListRow {post} />
			{/each}
		{/if}
	</div>
</section>
