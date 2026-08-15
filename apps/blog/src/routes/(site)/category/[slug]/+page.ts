import { fetchPosts } from '$lib/api';
import { error } from '@sveltejs/kit';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch, parent }) => {
	const { categories } = await parent();
	const category = categories.find((c) => c.slug === params.slug);

	if (!category) {
		error(404, 'Category not found');
	}

	// No `author` returned here — the Author File sidebar card only shows up
	// on the article reading page, where "this article's author" is
	// unambiguous. A category has many authors, so it stays hidden here.
	const categoryPosts = await fetchPosts(fetch, category.slug);

	return { category, categoryPosts };
};
