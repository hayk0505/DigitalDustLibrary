const KEY = 'dd-blog-reading-history';

const MIN_TRACK_PERCENT = 2;
const FINISHED_PERCENT = 95;

class ReadingHistoryState {
	entries = $state<Record<string, number>>({});

	load() {
		const stored = localStorage.getItem(KEY);
		if (!stored) return;
		try {
			const parsed = JSON.parse(stored);
			if (parsed && typeof parsed === 'object' && !Array.isArray(parsed)) {
				this.entries = parsed;
			}
		} catch {}
	}

	getProgress(slug: string): number | null {
		const value = this.entries[slug];
		if (value === undefined || value < MIN_TRACK_PERCENT || value >= FINISHED_PERCENT) return null;
		return value;
	}

	record(slug: string, progress: number) {
		if (progress < MIN_TRACK_PERCENT || progress >= FINISHED_PERCENT) {
			if (slug in this.entries) {
				const next = { ...this.entries };
				delete next[slug];
				this.entries = next;
				this.persist();
			}
			return;
		}
		this.entries = { ...this.entries, [slug]: Math.round(progress) };
		this.persist();
	}

	private persist() {
		localStorage.setItem(KEY, JSON.stringify(this.entries));
	}
}

export const readingHistory = new ReadingHistoryState();
