import { fetchPostBySlug, fetchAuthorByHandle, fetchPosts } from '$lib/api';
import { error } from '@sveltejs/kit';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
	const post = await fetchPostBySlug(fetch, params.slug);

	if (!post) {
		error(404, 'Post not found');
	}

	// The Author File sidebar card only ever shows up on article pages,
	// where "this article's author" is unambiguous — the layout provides
	// no author/posts of its own. Same fetch-all-then-filter shape as
	// authors/[handle]/+page.ts uses for the same "this author's posts" need.
	const [author, allPosts] = await Promise.all([
		fetchAuthorByHandle(fetch, post.authorHandle),
		fetchPosts(fetch)
	]);
	const posts = allPosts.filter((p) => p.authorHandle === post.authorHandle);

	// Reader mode's prev/next nav walks the same category in dispatch
	// order (oldest = 1, same numbering the rest of the site already uses
	// for that category) rather than site-wide publish date, so moving
	// between articles stays on-topic instead of jumping genres.
	const categoryPosts = allPosts
		.filter((p) => p.categorySlug === post.categorySlug)
		.sort((a, b) => a.dispatchNumber - b.dispatchNumber);
	const currentIndex = categoryPosts.findIndex((p) => p.slug === post.slug);
	const prevPost = currentIndex > 0 ? categoryPosts[currentIndex - 1] : null;
	const nextPost =
		currentIndex !== -1 && currentIndex < categoryPosts.length - 1
			? categoryPosts[currentIndex + 1]
			: null;

	// Related Articles (normal view only, not reader mode -- see
	// articles/[slug]/+page.svelte). Scored rather than a straight
	// category filter: a shared tag is a stronger relevance signal than
	// just sharing a category, so it's weighted higher, but category
	// alone still counts for something -- most posts have no tags yet
	// (tags are a new feature), and without that fallback this section
	// would be empty on nearly everything today. No editorial-override
	// layer yet (e.g. a manually curated list beating the computed one)
	// -- noted as the natural next step, not needed for a first version.
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
