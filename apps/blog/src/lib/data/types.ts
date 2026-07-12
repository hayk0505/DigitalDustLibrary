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
	role: string;
	avatarColor: string;
};

export type Post = {
	slug: string;
	title: string;
	excerpt: string;
	pillarSlug: string;
	authorHandle: string;
	publishedAt: string;
	readingMinutes: number;
	dispatchNumber: number;
	featured: boolean;
	body: string;
};
