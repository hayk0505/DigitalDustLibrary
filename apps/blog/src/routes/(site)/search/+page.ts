import { fetchSearchResults } from '$lib/api';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ url, fetch }) => {
	const query = url.searchParams.get('q')?.trim() ?? '';
	const results = query.length >= 2 ? await fetchSearchResults(fetch, query) : [];
	return { query, results };
};
