import { fetchSettings } from '$lib/api';
import type { LayoutLoad } from './$types';

export const load: LayoutLoad = async ({ fetch }) => {
	const settings = await fetchSettings(fetch);
	return { settings };
};
