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

	return { post, author, posts };
};
