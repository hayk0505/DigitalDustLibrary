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
	categoryFolderColor: string | null;
	authorHandle: string;
	authorName: string;
	publishedAt: string;
	readingMinutes: number;
	dispatchNumber: number;
};

// Sidebar turntable track — see GET /api/public/audio (PublicEndpoints.cs)
// and AudioTrackScanner.cs for how these get built from whatever's sitting
// in the droplet's audio-files/ folder. No local static/audio/ scan anymore;
// see $lib/api.ts's fetchPlaylist and $lib/data/playlist.ts.
export type Track = {
	id: string;
	title: string;
	artist: string;
	src: string;
	labelColor?: string;
};
