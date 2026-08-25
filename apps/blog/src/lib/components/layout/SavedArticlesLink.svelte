<script lang="ts">
	import { Bookmark } from 'lucide-svelte';
	import { savedArticles } from '$lib/state/saved-articles.svelte';

	let {
		class: buttonClass = 'text-ink/70 hover:text-ink',
		iconClass = 'h-4 w-4'
	}: { class?: string; iconClass?: string } = $props();

	const hasSaved = $derived(savedArticles.entries.length > 0);
</script>

<a href="/saved" aria-label="Saved articles" title="Saved articles" class={buttonClass}>
	<Bookmark class={iconClass} fill={hasSaved ? 'currentColor' : 'none'} aria-hidden="true" />
</a>
