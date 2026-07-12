import { posts } from './posts';
import type { Post } from './types';

export * from './types';
export { authors, getAuthorByHandle } from './authors';
export { getPillarBySlug, pillarAccentClasses, pillars } from './pillars';

export function getAllPosts(): Post[] {
	return [...posts].sort((a, b) => (a.publishedAt < b.publishedAt ? 1 : -1));
}

export function getPostsByPillar(pillarSlug: string): Post[] {
	return getAllPosts().filter((post) => post.pillarSlug === pillarSlug);
}

export function getFeaturedPostForPillar(pillarSlug: string): Post | undefined {
	return posts.find((post) => post.pillarSlug === pillarSlug && post.featured);
}

export function getNonFeaturedPostsForPillar(pillarSlug: string): Post[] {
	return getPostsByPillar(pillarSlug).filter((post) => !post.featured);
}

export function getPostBySlug(slug: string): Post | undefined {
	return posts.find((post) => post.slug === slug);
}

export function getRelatedPosts(post: Post, limit = 3): Post[] {
	return getPostsByPillar(post.pillarSlug)
		.filter((candidate) => candidate.slug !== post.slug)
		.slice(0, limit);
}

export function getPostsByAuthor(handle: string): Post[] {
	return getAllPosts().filter((post) => post.authorHandle === handle);
}
