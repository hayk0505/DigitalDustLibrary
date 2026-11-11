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

const HEX_RE = /^#[0-9a-f]{6}$/i;

// Highest WCAG relative luminance a folder tone can have and still carry the
// light folder ink (#f4f1ea) at ~5:1 contrast. Shared by getCategoryTextColor
// and getFolderTone so the two can never disagree about which side of the
// line a color is on.
const LIGHT_INK_MAX_LUMINANCE = 0.13;

function parseHex(hex: string): [number, number, number] {
	return [
		parseInt(hex.slice(1, 3), 16),
		parseInt(hex.slice(3, 5), 16),
		parseInt(hex.slice(5, 7), 16)
	];
}

function toHex([r, g, b]: [number, number, number]): string {
	return '#' + [r, g, b].map((c) => Math.round(c).toString(16).padStart(2, '0')).join('');
}

function relativeLuminance(hex: string): number {
	const [rl, gl, bl] = parseHex(hex).map((c) => {
		const v = c / 255;
		return v <= 0.03928 ? v / 12.92 : ((v + 0.055) / 1.055) ** 2.4;
	});
	return 0.2126 * rl! + 0.7152 * gl! + 0.0722 * bl!;
}

// Relative-luminance contrast pick (WCAG formula) rather than a hardcoded
// light/dark choice. Folder tabs go through getFolderTone first, so for them
// this always lands on the light ink; the dark branch is kept for any caller
// that passes a raw, unadjusted color.
export function getCategoryTextColor(hex: string): string {
	return relativeLuminance(hex) > LIGHT_INK_MAX_LUMINANCE ? '#2a281f' : '#f4f1ea';
}

// Background tone for anything that puts text or an icon on a category color
// (folder tabs, spines, badges, placeholder tiles). Folder labels are always
// light ink by design, so an admin-set folderColor too light for that
// (e.g. Culture's #deabbd) is darkened toward black instead of flipping the
// label to dark text. Scaling all three sRGB channels by the same factor
// keeps the hue; the binary search keeps the result as close to the picked
// color as the contrast allows. Accent-only uses (dots, rules) keep the raw
// folderColor and don't need this.
export function getFolderTone(folderColor: string | null | undefined, slug: string): string {
	const base = folderColor ?? getCategoryTabColor(slug);
	if (!HEX_RE.test(base) || relativeLuminance(base) <= LIGHT_INK_MAX_LUMINANCE) return base;

	const rgb = parseHex(base);
	const scaled = (k: number) => toHex([rgb[0] * k, rgb[1] * k, rgb[2] * k]);
	let lo = 0;
	let hi = 1;
	for (let i = 0; i < 16; i++) {
		const mid = (lo + hi) / 2;
		// Measured on the rounded hex, so the returned color is guaranteed
		// to sit at or under the threshold, not just the unrounded math.
		if (relativeLuminance(scaled(mid)) <= LIGHT_INK_MAX_LUMINANCE) lo = mid;
		else hi = mid;
	}
	return scaled(lo);
}
