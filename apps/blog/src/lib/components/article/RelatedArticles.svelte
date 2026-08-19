<script lang="ts">
	import type { Post } from '$lib/data';
	import { getCategoryTabColor } from '$lib/utils/category-visuals';

	let { posts }: { posts: Post[] } = $props();
</script>

{#if posts.length > 0}
	<section class="mt-4 border-t border-ink/10 pt-8">
		<h2 class="font-label text-xs tracking-widest text-ink/50 uppercase">Related Articles</h2>
		<div class="mt-4 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
			{#each posts as post (post.slug)}
				{@const accentColor = post.categoryFolderColor ?? getCategoryTabColor(post.categorySlug)}
				<a
					href="/articles/{post.slug}"
					class="related-card block rounded-lg border border-ink/15 bg-paper p-4 text-ink transition-colors hover:border-ink/30"
				>
					<span class="accent-text font-label text-xs tracking-widest uppercase" style="--accent: {accentColor}">
						{post.categoryName}
					</span>
					<h3 class="mt-2 font-display text-base font-bold">{post.title}</h3>
					<p class="mt-3 font-label text-xs text-ink/50">{post.readingMinutes} min read</p>
				</a>
			{/each}
		</div>
	</section>
{/if}
