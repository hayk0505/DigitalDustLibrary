<script lang="ts">
	import { History } from 'lucide-svelte';
	import type { Post } from '$lib/data';
	import { recentSearches, type RecentSearch } from '$lib/state/recent-searches.svelte';
	import SearchResultItem from './SearchResultItem.svelte';
	import CategoryDot from '../shared/CategoryDot.svelte';

	let {
		query,
		results,
		suggested,
		loading,
		activeIndex,
		onSelectRecent,
		onSelectResult,
		onViewAll
	}: {
		query: string;
		results: Post[] | null;
		suggested: Post[];
		loading: boolean;
		activeIndex: number;
		onSelectRecent: (entry: RecentSearch) => void;
		onSelectResult: (post: Post) => void;
		onViewAll: () => void;
	} = $props();
</script>

<span class="font-label text-xs tracking-widest text-ink/50 uppercase">Search Library</span>

{#if results === null}
	{#if recentSearches.entries.length > 0}
		<div class="mt-4">
			<div class="flex items-center justify-between">
				<span class="font-label text-xs tracking-widest text-ink/50 uppercase">Recent Searches</span
				>
				<button
					type="button"
					onclick={() => recentSearches.clear()}
					class="font-label text-xs tracking-widest text-ink/50 uppercase hover:text-ink"
				>
					Clear
				</button>
			</div>
			<ul class="mt-2">
				{#each recentSearches.entries as entry (entry.query)}
					<li>
						<button
							type="button"
							onclick={() => onSelectRecent(entry)}
							class="flex w-full items-center justify-between gap-2 py-1.5 text-left hover:text-accent-red"
						>
							<span class="flex items-center gap-2 text-sm">
								<History class="h-3.5 w-3.5 shrink-0 text-ink/40" aria-hidden="true" />
								{entry.query}
							</span>
							{#if entry.categoryName}
								<span
									class="flex shrink-0 items-center gap-1.5 font-label text-[11px] tracking-widest text-ink/50 uppercase"
								>
									{#if entry.categoryColor}<CategoryDot color={entry.categoryColor} />{/if}
									{entry.categoryName}
								</span>
							{/if}
						</button>
					</li>
				{/each}
			</ul>
		</div>
	{/if}

	{#if suggested.length > 0}
		<div class="mt-4">
			<span class="font-label text-xs tracking-widest text-ink/50 uppercase">Suggested</span>
			{#each suggested as post, i (post.slug)}
				<SearchResultItem
					{post}
					index={i}
					compact
					active={i === activeIndex}
					onSelect={() => onSelectResult(post)}
				/>
			{/each}
		</div>
	{/if}
{:else}
	<div class="mt-4">
		<span class="font-label text-xs tracking-widest text-ink/50 uppercase">
			{loading ? 'Searching…' : `Search Results (${results.length})`}
		</span>
		{#each results as post, i (post.slug)}
			<SearchResultItem
				{post}
				index={i}
				compact
				active={i === activeIndex}
				onSelect={() => onSelectResult(post)}
			/>
		{/each}
		{#if results.length === 0 && !loading}
			<p class="mt-2 text-sm text-ink/60">No results for "{query}".</p>
		{/if}
	</div>

	{#if results.length > 0}
		<button
			type="button"
			onclick={onViewAll}
			class="mt-3 flex w-full items-center justify-center gap-1 border-t border-ink/10 pt-3 font-label text-xs tracking-widest text-accent-red uppercase hover:underline"
		>
			View all results →
		</button>
	{/if}
{/if}
