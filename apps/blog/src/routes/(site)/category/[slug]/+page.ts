import { fetchPosts } from '$lib/api';
import { error } from '@sveltejs/kit';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch, url, parent }) => {
	const { categories } = await parent();
	const category = categories.find((c) => c.slug === params.slug);

	if (!category) {
		error(404, 'Category not found');
	}

	const categoryPosts = await fetchPosts(fetch, category.slug);

	const type = url.searchParams.get('type');
	const typeParam = (type === 'mini' || type === 'shelf' ? type : 'all') as
		'all' | 'mini' | 'shelf';
	const filteredPosts =
		typeParam === 'all' ? categoryPosts : categoryPosts.filter((post) => post.format === typeParam);

	return { category, categoryPosts: filteredPosts, typeParam };
};
