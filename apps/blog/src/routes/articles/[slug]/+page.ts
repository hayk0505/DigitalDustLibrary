import { fetchPostBySlug } from '$lib/api';
import { error } from '@sveltejs/kit';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
	const post = await fetchPostBySlug(fetch, params.slug);

	if (!post) {
		error(404, 'Post not found');
	}

	return { post };
};
