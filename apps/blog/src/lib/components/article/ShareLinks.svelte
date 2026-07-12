<script lang="ts">
	let { url }: { url: string } = $props();

	const linkedInHref = $derived(
		`https://www.linkedin.com/sharing/share-offsite/?url=${encodeURIComponent(url)}`
	);
</script>

<div class="flex items-center gap-3 font-label text-xs tracking-widest uppercase">
	<span class="text-ink/50">Share</span>
	<a
		href={linkedInHref}
		target="_blank"
		rel="noopener noreferrer"
		class="rounded border border-ink/20 px-2 py-1 hover:border-ink"
	>
		in
	</a>
</div>
