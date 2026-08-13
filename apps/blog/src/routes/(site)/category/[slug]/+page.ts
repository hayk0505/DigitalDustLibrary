import { fetchCategories, fetchPosts } from '$lib/api';
import { error } from '@sveltejs/kit';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
	const categories = await fetchCategories(fetch);
	const category = categories.find((c) => c.slug === params.slug);

	if (!category) {
		error(404, 'Category not found');
	}

	const posts = await fetchPosts(fetch, category.slug);

	return { category, posts };
};
