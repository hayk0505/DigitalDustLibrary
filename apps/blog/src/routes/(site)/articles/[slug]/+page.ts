import { fetchPostBySlug, fetchAuthorByHandle, fetchPosts } from '$lib/api';
import { error } from '@sveltejs/kit';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
	const post = await fetchPostBySlug(fetch, params.slug);

	if (!post) {
		error(404, 'Post not found');
	}


	const [author, allPosts] = await Promise.all([
		fetchAuthorByHandle(fetch, post.authorHandle),
		fetchPosts(fetch)
	]);
	const posts = allPosts.filter((p) => p.authorHandle === post.authorHandle);

	const categoryPosts = allPosts
		.filter((p) => p.categorySlug === post.categorySlug)
		.sort((a, b) => Date.parse(a.publishedAt) - Date.parse(b.publishedAt));
	const currentIndex = categoryPosts.findIndex((p) => p.slug === post.slug);
	const prevPost = currentIndex > 0 ? categoryPosts[currentIndex - 1] : null;
	const nextPost =
		currentIndex !== -1 && currentIndex < categoryPosts.length - 1
			? categoryPosts[currentIndex + 1]
			: null;

	const tagSlugs = new Set(post.tags.map((t) => t.slug));
	const relatedPosts = allPosts
		.filter((p) => p.slug !== post.slug)
		.map((p) => {
			const sharedTags = p.tags.filter((t) => tagSlugs.has(t.slug)).length;
			const sameCategory = p.categorySlug === post.categorySlug ? 1 : 0;
			return { post: p, score: sharedTags * 10 + sameCategory };
		})
		.filter((entry) => entry.score > 0)
		.sort((a, b) => b.score - a.score || Date.parse(b.post.publishedAt) - Date.parse(a.post.publishedAt))
		.slice(0, 3)
		.map((entry) => entry.post);

	return { post, author, posts, prevPost, nextPost, relatedPosts };
};
