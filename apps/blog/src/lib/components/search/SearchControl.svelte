<script lang="ts">
	import { flushSync } from 'svelte';
	import { fly, fade } from 'svelte/transition';
	import { goto } from '$app/navigation';
	import { Search, X } from 'lucide-svelte';
	import { fetchSearchResults, fetchPosts } from '$lib/api';
	import { recentSearches, type RecentSearch } from '$lib/state/recent-searches.svelte';
	import SearchResultsBody from './SearchResultsBody.svelte';
	import type { Post } from '$lib/data';

	let open = $state(false);
	let query = $state('');
	let results = $state<Post[] | null>(null);
	let loading = $state(false);
	let suggested = $state<Post[]>([]);
	let desktopTriggerEl = $state<HTMLButtonElement | null>(null);
	let mobileTriggerEl = $state<HTMLButtonElement | null>(null);
	let desktopInputEl = $state<HTMLInputElement | null>(null);
	let mobileInputEl = $state<HTMLInputElement | null>(null);
	let activeIndex = $state(-1);

	const visibleList = $derived(results ?? suggested);

	function openSearch() {
		open = true;
	}

	function closeSearch() {
		open = false;
		flushSync();
		if (window.matchMedia('(min-width: 768px)').matches) {
			desktopTriggerEl?.focus();
		} else {
			mobileTriggerEl?.focus();
		}
	}

	$effect(() => {
		if (!open) return;
		recentSearches.load();
		fetchPosts(fetch, undefined, 3).then((posts) => (suggested = posts));
	});

	$effect(() => {
		if (!open) return;
		if (window.matchMedia('(min-width: 768px)').matches) {
			desktopInputEl?.focus();
		} else {
			mobileInputEl?.focus();
		}
	});

	function runSearch(q: string) {
		const trimmed = q.trim();
		if (trimmed.length < 2) {
			results = null;
			return;
		}
		loading = true;
		fetchSearchResults(fetch, trimmed).then((r) => {
			results = r;
			loading = false;
		});
	}

	$effect(() => {
		const q = query;
		activeIndex = -1;
		if (q.trim().length < 2) {
			results = null;
			return;
		}
		loading = true;
		const timer = setTimeout(() => runSearch(q), 250);
		return () => clearTimeout(timer);
	});

	function recordAndGo(url: string, topResult?: Post) {
		recentSearches.add(query, topResult ?? results?.[0]);
		goto(url);
		closeSearch();
	}

	function executeFullSearch() {
		if (query.trim().length < 2) return;
		recordAndGo(`/search?q=${encodeURIComponent(query.trim())}`);
	}

	function selectRecent(entry: RecentSearch) {
		query = entry.query;
		runSearch(entry.query);
	}

	function selectResult(post: Post) {
		recentSearches.add(query, post);
		closeSearch();
	}

	function onInputKeydown(e: KeyboardEvent) {
		if (e.key === 'ArrowDown' && visibleList.length > 0) {
			e.preventDefault();
			activeIndex = Math.min(activeIndex + 1, visibleList.length - 1);
			return;
		}
		if (e.key === 'ArrowUp' && visibleList.length > 0) {
			e.preventDefault();
			activeIndex = Math.max(activeIndex - 1, -1);
			return;
		}
		if (e.key === 'Enter') {
			e.preventDefault();
			const active = activeIndex >= 0 ? visibleList[activeIndex] : undefined;
			if (active) {
				recordAndGo(`/articles/${active.slug}`, active);
			} else {
				executeFullSearch();
			}
		}
	}

	function onGlobalKeydown(e: KeyboardEvent) {
		const target = e.target as HTMLElement | null;
		const inField =
			target?.tagName === 'INPUT' || target?.tagName === 'TEXTAREA' || target?.isContentEditable;

		if ((e.metaKey || e.ctrlKey) && e.key.toLowerCase() === 'k') {
			e.preventDefault();
			open = true;
			return;
		}
		if (e.key === '/' && !inField && !open) {
			e.preventDefault();
			open = true;
			return;
		}
		if (e.key === 'Escape' && open) {
			closeSearch();
		}
	}
</script>

<svelte:window onkeydown={onGlobalKeydown} />

<div class="relative hidden md:block">
	<div
		class="relative z-30 flex items-center justify-end overflow-hidden transition-[width] duration-200 ease-out {open
			? 'w-[360px]'
			: 'w-9'}"
	>
		{#if !open}
			<button
				type="button"
				bind:this={desktopTriggerEl}
				onclick={openSearch}
				aria-label="Search"
				class="search-btn flex h-9 w-9 shrink-0 items-center justify-center rounded-full"
				transition:fade={{ duration: 120 }}
			>
				<Search class="h-4 w-4" aria-hidden="true" />
			</button>
		{:else}
			<div
				class="flex h-9 w-full items-center gap-2 rounded-lg border border-ink/15 bg-paper px-3"
				transition:fade={{ duration: 120 }}
			>
				<Search class="h-4 w-4 shrink-0 text-ink/40" aria-hidden="true" />
				<input
					bind:this={desktopInputEl}
					type="text"
					bind:value={query}
					onkeydown={onInputKeydown}
					placeholder="Search the library…"
					aria-label="Search the library"
					class="w-full bg-transparent font-label text-sm outline-none placeholder:text-ink/40"
				/>
				<span class="shrink-0 font-label text-[10px] tracking-widest text-ink/40 uppercase"
					>Esc</span
				>
			</div>
		{/if}
	</div>

	{#if open}
		<button type="button" class="fixed inset-0 z-20" onclick={closeSearch} aria-label="Close search"
		></button>
		<div
			class="search-panel absolute right-0 z-30 mt-2 max-h-[70vh] w-[360px] overflow-y-auto p-4"
			transition:fly={{ y: -4, duration: 200 }}
		>
			<SearchResultsBody
				{query}
				{results}
				{suggested}
				{loading}
				{activeIndex}
				onSelectRecent={selectRecent}
				onSelectResult={selectResult}
				onViewAll={executeFullSearch}
			/>
		</div>
	{/if}
</div>
<div class="relative flex md:hidden">
	<button
		type="button"
		bind:this={mobileTriggerEl}
		onclick={openSearch}
		aria-label="Search"
		aria-haspopup="dialog"
		aria-expanded={open}
		class="search-btn flex h-[38px] w-[38px] items-center justify-center rounded-full"
	>
		<Search class="h-4 w-4" aria-hidden="true" />
	</button>

	{#if open}
		<button type="button" class="fixed inset-0 z-20" onclick={closeSearch} aria-label="Close search"
		></button>
		<div
			class="search-panel fixed inset-x-3 top-16 z-30 max-h-[70vh] overflow-y-auto p-4"
			transition:fly={{ y: -4, duration: 200 }}
		>
			<div class="flex items-center gap-2 border-b border-ink/10 pb-2">
				<Search class="h-4 w-4 shrink-0 text-ink/40" aria-hidden="true" />
				<input
					bind:this={mobileInputEl}
					type="text"
					bind:value={query}
					onkeydown={onInputKeydown}
					placeholder="Search the library…"
					aria-label="Search the library"
					class="w-full bg-transparent font-label text-sm outline-none placeholder:text-ink/40"
				/>
				{#if query}
					<button
						type="button"
						onclick={() => (query = '')}
						aria-label="Clear search"
						class="text-ink/40 hover:text-ink"
					>
						<X class="h-3.5 w-3.5" aria-hidden="true" />
					</button>
				{/if}
			</div>
			<div class="mt-3">
				<SearchResultsBody
					{query}
					{results}
					{suggested}
					{loading}
					{activeIndex}
					onSelectRecent={selectRecent}
					onSelectResult={selectResult}
					onViewAll={executeFullSearch}
				/>
			</div>
		</div>
	{/if}
</div>
