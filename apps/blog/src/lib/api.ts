import { API_URL } from './config';
import type { Author, Post } from './data/types';

type ApiPost = {
	slug: string;
	title: string;
	bodyHtml: string;
	excerpt: string;
	seoTitle: string;
	metaDescription: string;
	featuredImageUrl: string | null;
	pillar: string;
	authorHandle: string;
	authorName: string;
	publishedAt: string;
	readingMinutes: number;
	dispatchNumber: number;
};

function toPost(api: ApiPost): Post {
	return {
		slug: api.slug,
		title: api.title,
		excerpt: api.excerpt,
		bodyHtml: api.bodyHtml,
		seoTitle: api.seoTitle,
		metaDescription: api.metaDescription,
		featuredImageUrl: api.featuredImageUrl,
		pillarSlug: api.pillar,
		authorHandle: api.authorHandle,
		authorName: api.authorName,
		publishedAt: api.publishedAt,
		readingMinutes: api.readingMinutes,
		dispatchNumber: api.dispatchNumber
	};
}

export async function fetchPosts(fetchFn: typeof fetch): Promise<Post[]> {
	const response = await fetchFn(`${API_URL}/posts`);
	if (!response.ok) throw new Error(`Failed to fetch posts: ${response.status}`);
	const posts: ApiPost[] = await response.json();
	return posts.map(toPost);
}

export async function fetchPostBySlug(fetchFn: typeof fetch, slug: string): Promise<Post | null> {
	const response = await fetchFn(`${API_URL}/posts/${slug}`);
	if (response.status === 404) return null;
	if (!response.ok) throw new Error(`Failed to fetch post: ${response.status}`);
	const post: ApiPost = await response.json();
	return toPost(post);
}

export async function fetchAuthorByHandle(
	fetchFn: typeof fetch,
	handle: string
): Promise<Author | null> {
	const response = await fetchFn(`${API_URL}/authors/${handle}`);
	if (response.status === 404) return null;
	if (!response.ok) throw new Error(`Failed to fetch author: ${response.status}`);
	return response.json();
}
