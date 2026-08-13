const MIN_HEIGHT = 220;
const MAX_HEIGHT = 420;
const TALL_THRESHOLD = 340;

// Same rolling-hash shape as avatar-color.ts's getAvatarColor, seeded by
// post.slug instead of a user handle — deterministic per post so SSR
// output and client hydration always agree (no Math.random() jump).
function hashString(seed: string): number {
	let hash = 0;
	for (let i = 0; i < seed.length; i++) {
		hash = (hash * 31 + seed.charCodeAt(i)) | 0;
	}
	return Math.abs(hash);
}

export function getCardHeight(slug: string): number {
	const fraction = (hashString(slug) % 1000) / 1000;
	return Math.round(MIN_HEIGHT + fraction * (MAX_HEIGHT - MIN_HEIGHT));
}

export function isTallCard(slug: string): boolean {
	return getCardHeight(slug) >= TALL_THRESHOLD;
}
