<script lang="ts">
	import type { Author } from '$lib/data';
	import Avatar from './Avatar.svelte';

	let { author, meta }: { author: Author; meta?: string } = $props();
</script>

<div class="flex items-center gap-2">
	<Avatar name={author.name} colorClass={author.avatarColor} />
	<div class="leading-tight">
		<p class="text-sm font-semibold">{author.name}</p>
		{#if meta}
			<p class="font-label text-[11px] tracking-wide text-ink/60 uppercase">{meta}</p>
		{/if}
	</div>
</div>
