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

	const allColumns = categories.map((category) => ({
		category,
		posts: posts.filter((post) => post.categorySlug === category.slug)
	}));

	const type = url.searchParams.get('type');
	const showMinis = type !== 'shelf';
	const showShelf = type !== 'mini';

	return {
		columns,
		allColumns,
		minis: posts.filter((post) => post.format === 'mini'),
		shelfPosts: posts.filter((post) => post.format === 'shelf'),
		showMinis,
		showShelf,
		typeParam: (type === 'mini' || type === 'shelf' ? type : 'all') as 'all' | 'mini' | 'shelf'
	};
};
