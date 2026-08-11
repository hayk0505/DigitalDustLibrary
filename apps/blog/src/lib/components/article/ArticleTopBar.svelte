<script lang="ts">
	import ShareLinks from './ShareLinks.svelte';
	import ReadingProgressBar from './ReadingProgressBar.svelte';

	let { shareUrl }: { shareUrl: string } = $props();
</script>

<div class="sticky top-0 z-20 bg-paper">
	<div class="border-b border-ink/10">
		<div class="mx-auto flex max-w-3xl items-center justify-between px-6 py-4">
			<a href="/" class="font-label text-xs tracking-widest text-ink/70 uppercase hover:text-ink">
				← All columns
			</a>
			<a href="/" class="font-brand text-sm font-bold tracking-tight uppercase">
				Digital Dust <span class="text-accent-red">Library</span>
			</a>
			<ShareLinks url={shareUrl} />
		</div>
	</div>
	<ReadingProgressBar />
</div>
