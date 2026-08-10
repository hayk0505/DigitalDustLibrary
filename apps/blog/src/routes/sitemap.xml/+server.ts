import { fetchPosts } from '$lib/api';
import type { RequestHandler } from './$types';

export const GET: RequestHandler = async ({ fetch, url }) => {
	const posts = await fetchPosts(fetch);

	const staticPaths = ['/', '/archive', '/become-an-author'];
	const authorHandles = [...new Set(posts.map((post) => post.authorHandle))];

	const staticUrls = staticPaths.map(
		(path) => `
	<url>
		<loc>${url.origin}${path}</loc>
	</url>`
	);

	const postUrls = posts.map(
		(post) => `
	<url>
		<loc>${url.origin}/articles/${post.slug}</loc>
		<lastmod>${post.publishedAt.slice(0, 10)}</lastmod>
	</url>`
	);

	const authorUrls = authorHandles.map(
		(handle) => `
	<url>
		<loc>${url.origin}/authors/${handle}</loc>
	</url>`
	);

	const xml = `<?xml version="1.0" encoding="UTF-8"?>
<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">${[...staticUrls, ...postUrls, ...authorUrls].join('')}
</urlset>`;

	return new Response(xml, {
		headers: { 'Content-Type': 'application/xml' }
	});
};
