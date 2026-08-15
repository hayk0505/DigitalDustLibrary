import { fetchPosts } from '$lib/api';
import type { PageLoad } from './$types';

const CATEGORIES_PER_PAGE = 3;

export const load: PageLoad = async ({ fetch, url, parent }) => {
	const [{ categories }, posts] = await Promise.all([parent(), fetchPosts(fetch)]);

	const catPage = Math.max(0, Number(url.searchParams.get('catPage') ?? '0') || 0);
	const totalPages = Math.max(1, Math.ceil(categories.length / CATEGORIES_PER_PAGE));
	const clampedPage = Math.min(catPage, totalPages - 1);
	const pageCategories = categories.slice(
		clampedPage * CATEGORIES_PER_PAGE,
		clampedPage * CATEGORIES_PER_PAGE + CATEGORIES_PER_PAGE
	);

	const columns = pageCategories.map((category) => ({
		category,
		posts: posts.filter((post) => post.categorySlug === category.slug)
	}));

	return { columns };
};
