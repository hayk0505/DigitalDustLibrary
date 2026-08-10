// Deterministic hash of a seed (an author's handle) into one of a fixed
// palette of Tailwind background classes — replaces the mock data's
// per-author `avatarColor` field, which has no backend equivalent (the
// public API only ever returns a handle/name, nothing presentational).
const PALETTE = [
	'bg-amber-500',
	'bg-rose-500',
	'bg-emerald-500',
	'bg-violet-500',
	'bg-sky-500',
	'bg-fuchsia-500',
	'bg-blue-600',
	'bg-teal-500'
];

export function getAvatarColor(seed: string): string {
	let hash = 0;
	for (let i = 0; i < seed.length; i++) {
		hash = (hash * 31 + seed.charCodeAt(i)) | 0;
	}
	return PALETTE[Math.abs(hash) % PALETTE.length]!;
}
