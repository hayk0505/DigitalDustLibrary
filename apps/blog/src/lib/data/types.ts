export type Category = {
	slug: string;
	name: string;
	description: string;
	color: string;
	position: number;
	postCount: number;
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
	categorySlug: string;
	categoryName: string;
	categoryColor: string;
	authorHandle: string;
	authorName: string;
	publishedAt: string;
	readingMinutes: number;
	dispatchNumber: number;
};
