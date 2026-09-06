const PALETTE = [
	'bg-amber-700',
	'bg-rose-700',
	'bg-emerald-700',
	'bg-violet-600',
	'bg-sky-700',
	'bg-fuchsia-700',
	'bg-blue-600',
	'bg-teal-700'
];

export function getAvatarColor(seed: string): string {
	let hash = 0;
	for (let i = 0; i < seed.length; i++) {
		hash = (hash * 31 + seed.charCodeAt(i)) | 0;
	}
	return PALETTE[Math.abs(hash) % PALETTE.length]!;
}
