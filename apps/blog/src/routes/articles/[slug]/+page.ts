import { getAuthorByHandle, getPostBySlug } from '$lib/data';
import { error } from '@sveltejs/kit';
import type { PageLoad } from './$types';

export const load: PageLoad = ({ params }) => {
	const post = getPostBySlug(params.slug);

	if (!post) {
		error(404, 'Post not found');
	}

	const author = getAuthorByHandle(post.authorHandle);

	if (!author) {
		error(404, 'Author not found');
	}

	return { post, author };
};
