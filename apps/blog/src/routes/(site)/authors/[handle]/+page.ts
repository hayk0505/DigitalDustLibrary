import { fetchAuthorByHandle, fetchPosts } from '$lib/api';
import { error } from '@sveltejs/kit';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
	const author = await fetchAuthorByHandle(fetch, params.handle);

	if (!author) {
		error(404, 'Author not found');
	}

	const allPosts = await fetchPosts(fetch);
	const posts = allPosts.filter((post) => post.authorHandle === author.handle);

	return { author, posts };
};
