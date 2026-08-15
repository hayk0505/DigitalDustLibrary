import { fetchCategories } from '$lib/api';
import type { LayoutLoad } from './$types';

export const load: LayoutLoad = async ({ fetch }) => {
	return { categories: await fetchCategories(fetch) };
};
