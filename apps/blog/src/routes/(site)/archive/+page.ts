import { getAllPosts } from '$lib/data';
import type { PageLoad } from './$types';

export const load: PageLoad = () => {
	return { posts: getAllPosts() };
};
