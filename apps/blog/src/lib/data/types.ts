export type PillarAccent = 'red' | 'green' | 'blue';

export type Pillar = {
	slug: string;
	label: string;
	index: number;
	accent: PillarAccent;
};

export type Author = {
	handle: string;
	name: string;
};

export type Post = {
	slug: string;
	title: string;
	excerpt: string;
	bodyHtml: string;
	seoTitle: string;
	metaDescription: string;
	featuredImageUrl: string | null;
	pillarSlug: string;
	authorHandle: string;
	authorName: string;
	publishedAt: string;
	readingMinutes: number;
	dispatchNumber: number;
};
