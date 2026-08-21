import {
	ScanBarcode,
	Gamepad2,
	Brain,
	Code2,
	Globe,
	Leaf,
	FileText,
	Compass,
	Zap,
	Layers
} from 'lucide-svelte';

// Same rolling-hash shape as avatar-color.ts's getAvatarColor —
// deterministic per category slug, so the same category always gets the
// same icon/color, including ones that don't exist yet. Not a semantic
// match to the category's meaning, deliberately — categories are
// open-ended and admin-managed here, so there's no fixed list to
// hand-maintain.
function hashString(seed: string): number {
	let hash = 0;
	for (let i = 0; i < seed.length; i++) {
		hash = (hash * 31 + seed.charCodeAt(i)) | 0;
	}
	return Math.abs(hash);
}

type IconType = typeof ScanBarcode;

const ICONS: IconType[] = [
	ScanBarcode,
	Gamepad2,
	Brain,
	Code2,
	Globe,
	Leaf,
	FileText,
	Compass,
	Zap,
	Layers
];

// Muted "file folder" tones — deliberately unrelated to Category.color
// (the bright accent used for dots/hover elsewhere on the site). Darkened
// below getCategoryTextColor's 0.13 luminance threshold on purpose, so the
// folder label always resolves to the light/white ink branch — see that
// function's comment for why the color itself stays dynamic rather than a
// hardcoded white (admin-set custom folderColor can still be a light hex,
// and would correctly fall back to dark text).
const TAB_COLORS = [
	'#545F66', // slate blue-gray
	'#6F5836', // kraft tan
	'#3A3A36', // charcoal
	'#536147', // olive green
	'#675C47', // khaki
	'#7C5435', // warm brown
	'#525E6E', // dusty blue
	'#6A5A45' // muted taupe
];

export function getCategoryIcon(slug: string): IconType {
	return ICONS[hashString(slug) % ICONS.length]!;
}

export function getCategoryTabColor(slug: string): string {
	// Different modulus than getCategoryIcon (10 vs 8) on the same hash —
	// enough to decorrelate icon and color picks without needing a
	// second hash function.
	return TAB_COLORS[hashString(slug) % TAB_COLORS.length]!;
}

// Relative-luminance contrast pick (WCAG formula) rather than a hardcoded
// light/dark choice — folderColor is admin-settable to any hex now, not
// just the muted hashed palette above, so a fixed text color would go
// unreadable on some picks.
export function getCategoryTextColor(hex: string): string {
	const r = parseInt(hex.slice(1, 3), 16) / 255;
	const g = parseInt(hex.slice(3, 5), 16) / 255;
	const b = parseInt(hex.slice(5, 7), 16) / 255;
	const [rl, gl, bl] = [r, g, b].map((c) =>
		c <= 0.03928 ? c / 12.92 : ((c + 0.055) / 1.055) ** 2.4
	);
	const luminance = 0.2126 * rl! + 0.7152 * gl! + 0.0722 * bl!;
	return luminance > 0.13 ? '#2a281f' : '#f4f1ea';
}
