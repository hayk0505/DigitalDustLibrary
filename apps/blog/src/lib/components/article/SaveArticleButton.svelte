<script lang="ts">
	import { Bookmark } from 'lucide-svelte';
	import { savedArticles } from '$lib/state/saved-articles.svelte';
	import type { Post } from '$lib/data';

	let { post }: { post: Post } = $props();

	const saved = $derived(savedArticles.isSaved(post.slug));
</script>

<button
	type="button"
	onclick={() => savedArticles.toggle(post)}
	aria-label={saved ? 'Remove from saved articles' : 'Save article'}
	aria-pressed={saved}
	title={saved ? 'Saved' : 'Save for later'}
	class="flex items-center gap-1.5 font-label text-xs tracking-widest uppercase hover:text-accent-red"
>
	<Bookmark class="h-3.5 w-3.5" fill={saved ? 'currentColor' : 'none'} aria-hidden="true" />
	{saved ? 'Saved' : 'Save'}
</button>
