const ACTIVE_KEY = 'dd-blog-reader-mode';
const SETTINGS_KEY = 'dd-blog-reader-settings';

export const FONT_SIZE_MIN = 16;
export const FONT_SIZE_MAX = 22;
export const LINE_HEIGHT_MIN = 1.5;
export const LINE_HEIGHT_MAX = 2.0;
export const READING_WIDTH_MIN = 680;
export const READING_WIDTH_MAX = 760;

const DEFAULTS = { fontSize: 19, lineHeight: 1.75, readingWidth: 720 };

function clamp(n: number, min: number, max: number): number {
	return Math.min(max, Math.max(min, n));
}

/**
 * Reader Mode's on/off flag and text-display settings — one shared instance,
 * same module-state pattern as $lib/state/turntable.svelte.ts.
 *
 * The `active` flag is dual-tracked: this class's own $state field (for
 * Svelte components that branch on it, e.g. the article page swapping its
 * whole chrome) AND `<html data-reader-mode>` (for the plain-CSS rule in
 * app.css that hides the persistent sidebar/player/scroll-button — see
 * app.html's inline script, which stamps that attribute from localStorage
 * before hydration so a returning reader-mode user's first paint never
 * shows the full chrome before it disappears). enter()/exit() keep both in
 * sync; load() brings the reactive field into agreement with whatever the
 * inline script already decided, once this runs client-side.
 */
class ReaderModeState {
	active = $state(false);
	fontSize = $state(DEFAULTS.fontSize);
	lineHeight = $state(DEFAULTS.lineHeight);
	readingWidth = $state(DEFAULTS.readingWidth);

	load() {
		this.active = document.documentElement.dataset.readerMode === 'on';

		const stored = localStorage.getItem(SETTINGS_KEY);
		if (!stored) return;
		try {
			const parsed = JSON.parse(stored);
			if (typeof parsed.fontSize === 'number') {
				this.fontSize = clamp(parsed.fontSize, FONT_SIZE_MIN, FONT_SIZE_MAX);
			}
			if (typeof parsed.lineHeight === 'number') {
				this.lineHeight = clamp(parsed.lineHeight, LINE_HEIGHT_MIN, LINE_HEIGHT_MAX);
			}
			if (typeof parsed.readingWidth === 'number') {
				this.readingWidth = clamp(parsed.readingWidth, READING_WIDTH_MIN, READING_WIDTH_MAX);
			}
		} catch {
			// Malformed localStorage value (e.g. hand-edited) — keep defaults
			// rather than throw during layout mount.
		}
	}

	enter() {
		this.active = true;
		document.documentElement.dataset.readerMode = 'on';
		localStorage.setItem(ACTIVE_KEY, 'on');
	}

	exit() {
		this.active = false;
		document.documentElement.dataset.readerMode = 'off';
		localStorage.setItem(ACTIVE_KEY, 'off');
	}

	setFontSize(px: number) {
		this.fontSize = clamp(px, FONT_SIZE_MIN, FONT_SIZE_MAX);
		this.persistSettings();
	}

	setLineHeight(lh: number) {
		this.lineHeight = clamp(lh, LINE_HEIGHT_MIN, LINE_HEIGHT_MAX);
		this.persistSettings();
	}

	setReadingWidth(px: number) {
		this.readingWidth = clamp(px, READING_WIDTH_MIN, READING_WIDTH_MAX);
		this.persistSettings();
	}

	private persistSettings() {
		localStorage.setItem(
			SETTINGS_KEY,
			JSON.stringify({
				fontSize: this.fontSize,
				lineHeight: this.lineHeight,
				readingWidth: this.readingWidth
			})
		);
	}
}

export const readerMode = new ReaderModeState();
