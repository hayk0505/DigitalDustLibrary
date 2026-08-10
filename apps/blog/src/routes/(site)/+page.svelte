<script lang="ts">
	import { page } from '$app/state';
	import SeoHead from '$lib/components/shared/SeoHead.svelte';
	import IssueBanner from '$lib/components/home/IssueBanner.svelte';
	import PillarColumn from '$lib/components/home/PillarColumn.svelte';
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();
</script>

<SeoHead
	title="Digital Dust Library — Field notes on what the internet leaves behind"
	description="Field notes on what the internet leaves behind."
	url={page.url.href}
/>

<IssueBanner volumeLabel="Vol. 04 — July 2026" dispatchCount={data.totalCount} />

<div class="mt-8 grid gap-10 md:grid-cols-3">
	{#each data.columns as column (column.pillar.slug)}
		<PillarColumn pillar={column.pillar} posts={column.posts} />
	{/each}
</div>
