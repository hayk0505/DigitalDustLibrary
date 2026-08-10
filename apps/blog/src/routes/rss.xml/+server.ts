import { fetchPosts } from '$lib/api';
import { escapeXml } from '$lib/utils/xml';
import type { RequestHandler } from './$types';

const MAX_ITEMS = 50;

export const GET: RequestHandler = async ({ fetch, url }) => {
	const posts = await fetchPosts(fetch);
	const recent = posts.slice(0, MAX_ITEMS);

	const items = recent
		.map((post) => {
			const link = `${url.origin}/articles/${post.slug}`;
			return `
	<item>
		<title>${escapeXml(post.title)}</title>
		<link>${link}</link>
		<guid isPermaLink="true">${link}</guid>
		<pubDate>${new Date(post.publishedAt).toUTCString()}</pubDate>
		<description><![CDATA[${post.bodyHtml}]]></description>
	</item>`;
		})
		.join('');

	const xml = `<?xml version="1.0" encoding="UTF-8"?>
<rss version="2.0">
<channel>
	<title>Digital Dust Library — Field notes on what the internet leaves behind</title>
	<link>${url.origin}</link>
	<description>Field notes on what the internet leaves behind</description>${items}
</channel>
</rss>`;

	return new Response(xml, {
		headers: { 'Content-Type': 'application/rss+xml' }
	});
};
