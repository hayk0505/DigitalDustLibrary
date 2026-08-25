import { fetchPosts } from '$lib/api';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch, url }) => {
	const posts = await fetchPosts(fetch);

	const type = url.searchParams.get('type');
	const typeParam = (type === 'mini' || type === 'shelf' ? type : 'all') as
		'all' | 'mini' | 'shelf';
	const filteredPosts =
		typeParam === 'all' ? posts : posts.filter((post) => post.format === typeParam);

	return { posts: filteredPosts, typeParam };
};
