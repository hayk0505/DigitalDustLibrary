import { getPostsByPillar, pillars } from '$lib/data';
import type { PageLoad } from './$types';

export const load: PageLoad = () => {
	const columns = pillars.map((pillar) => ({
		pillar,
		posts: getPostsByPillar(pillar.slug)
	}));

	return { columns };
};
