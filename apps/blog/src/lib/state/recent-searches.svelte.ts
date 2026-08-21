import type { Post } from '$lib/data';

const KEY = 'dd-blog-recent-searches';
const MAX_ENTRIES = 5;

export type RecentSearch = { query: string; categoryName?: string; categoryColor?: string };

// Same class-based $state singleton pattern as reader-mode.svelte.ts —
// load() brings the reactive field into agreement with localStorage once
// this runs client-side; the try/catch guards a malformed hand-edited value.
class RecentSearchesState {
	entries = $state<RecentSearch[]>([]);

	load() {
		const stored = localStorage.getItem(KEY);
		if (!stored) return;
		try {
			const parsed = JSON.parse(stored);
			if (Array.isArray(parsed)) this.entries = parsed;
		} catch {
			// Malformed localStorage value — keep the empty default.
		}
	}

	// The category badge shown next to a recent search is the top-ranked
	// result's category at the time of that search — there's no independent
	// "category of a search" concept, so the best match stands in for one.
	add(query: string, topResult?: Post) {
		const trimmed = query.trim();
		if (!trimmed) return;
		const entry: RecentSearch = {
			query: trimmed,
			categoryName: topResult?.categoryName,
			categoryColor: topResult?.categoryColor
		};
		this.entries = [
			entry,
			...this.entries.filter((e) => e.query.toLowerCase() !== trimmed.toLowerCase())
		].slice(0, MAX_ENTRIES);
		this.persist();
	}

	clear() {
		this.entries = [];
		this.persist();
	}

	private persist() {
		localStorage.setItem(KEY, JSON.stringify(this.entries));
	}
}

export const recentSearches = new RecentSearchesState();
