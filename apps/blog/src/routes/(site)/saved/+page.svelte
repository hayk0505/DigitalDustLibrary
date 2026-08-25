<script lang="ts">
	import { page } from '$app/state';
	import { X } from 'lucide-svelte';
	import SeoHead from '$lib/components/shared/SeoHead.svelte';
	import PostTeaserRow from '$lib/components/home/PostTeaserRow.svelte';
	import { savedArticles } from '$lib/state/saved-articles.svelte';
	import { getCategoryTabColor } from '$lib/utils/category-visuals';
</script>

<SeoHead
	title="Saved Articles"
	description="Articles bookmarked for later on this device."
	url={page.url.href}
/>

<div class="flex items-center justify-between">
	<div>
		<h1 class="font-display text-3xl font-bold">Saved Articles</h1>
		<p class="mt-2 text-ink/60">
			Bookmarked on this device — clearing your browser data clears this list too.
		</p>
	</div>
	{#if savedArticles.entries.length > 0}
		<button
			type="button"
			onclick={() => savedArticles.clear()}
			class="shrink-0 font-label text-xs tracking-widest text-ink/60 uppercase hover:text-accent-red"
		>
			Clear all
		</button>
	{/if}
</div>

<div class="mt-8 max-w-2xl">
	{#if savedArticles.entries.length === 0}
		<p class="text-ink/60">
			No saved articles yet. Tap "Save" on an article to bookmark it for later.
		</p>
	{:else}
		{#each savedArticles.entries as post (post.slug)}
			{@const accentColor = post.categoryFolderColor ?? getCategoryTabColor(post.categorySlug)}
			<div class="relative">
				<PostTeaserRow {post} color={accentColor} />
				<button
					type="button"
					onclick={() => savedArticles.remove(post.slug)}
					aria-label="Remove {post.title} from saved articles"
					title="Remove from saved"
					class="absolute top-4 right-4 rounded-full border border-ink/20 bg-paper p-1 text-ink/50 hover:border-accent-red hover:text-accent-red"
				>
					<X class="h-3.5 w-3.5" aria-hidden="true" />
				</button>
			</div>
		{/each}
	{/if}
</div>
