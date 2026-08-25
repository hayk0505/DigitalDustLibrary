import type { Post } from '$lib/data';

const KEY = 'dd-blog-saved-articles';

class SavedArticlesState {
	entries = $state<Post[]>([]);

	load() {
		const stored = localStorage.getItem(KEY);
		if (!stored) return;
		try {
			const parsed = JSON.parse(stored);
			if (Array.isArray(parsed)) this.entries = parsed;
		} catch {}
	}

	isSaved(slug: string): boolean {
		return this.entries.some((p) => p.slug === slug);
	}

	toggle(post: Post) {
		if (this.isSaved(post.slug)) {
			this.remove(post.slug);
		} else {
			this.entries = [post, ...this.entries];
			this.persist();
		}
	}

	remove(slug: string) {
		this.entries = this.entries.filter((p) => p.slug !== slug);
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

export const savedArticles = new SavedArticlesState();
