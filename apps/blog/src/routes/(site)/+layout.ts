import { fetchCategories, fetchPlaylist } from '$lib/api';
import type { LayoutLoad } from './$types';

export const load: LayoutLoad = async ({ fetch }) => {
	const [categories, tracks] = await Promise.all([fetchCategories(fetch), fetchPlaylist(fetch)]);
	return { categories, tracks };
};
