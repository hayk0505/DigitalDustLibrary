import { getAuthorByHandle, getPostsByAuthor } from '$lib/data';
import { error } from '@sveltejs/kit';
import type { PageLoad } from './$types';

export const load: PageLoad = ({ params }) => {
	const author = getAuthorByHandle(params.handle);

	if (!author) {
		error(404, 'Author not found');
	}

	return { author, posts: getPostsByAuthor(author.handle) };
};
