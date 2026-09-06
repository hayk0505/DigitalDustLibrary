export type Category = {
	slug: string;
	name: string;
	description: string;
	color: string;
	folderColor: string | null;
	position: number;
	postCount: number;
};

export type Author = {
	handle: string;
	name: string;
	bio: string | null;
	createdAt: string;
};

export type PostTag = {
	name: string;
	slug: string;
};

export type Post = {
	slug: string;
	title: string;
	excerpt: string;
	bodyHtml: string;
	seoTitle: string;
	metaDescription: string;
	source: string | null;
	featuredImageUrl: string | null;
	categorySlug: string;
	categoryName: string;
	categoryColor: string;
	categoryFolderColor: string | null;
	authorHandle: string;
	authorName: string;
	publishedAt: string;
	readingMinutes: number;
	dispatchNumber: number;
	format: 'mini' | 'shelf';
	tags: PostTag[];
};


export type Track = {
	id: string;
	title: string;
	artist: string;
	src: string;
	labelColor?: string;
};

export type SiteSettings = {
	siteTitle: string;
	tagline: string;
	defaultMetaDescription: string;
	linkedInUrl: string;
	xUrl: string;
};
