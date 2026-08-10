import { fetchPosts } from '$lib/api';
import { pillars } from '$lib/data';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch }) => {
	const posts = await fetchPosts(fetch);

	const columns = pillars.map((pillar) => ({
		pillar,
		posts: posts.filter((post) => post.pillarSlug === pillar.slug)
	}));

	return { columns, totalCount: posts.length };
};
