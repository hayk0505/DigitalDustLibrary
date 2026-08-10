import { fetchPosts } from '$lib/api';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch }) => {
	return { posts: await fetchPosts(fetch) };
};
