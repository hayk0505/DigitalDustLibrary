import { getTheme, setTheme, type Theme } from '$lib/utils/theme';

/**
 * The active theme as one shared reactive value — same module-state pattern
 * as $lib/state/reader-mode.svelte.ts. Previously every theme toggle button
 * (BlogTopNav desktop+mobile, ArticleTopBar desktop+mobile, ReaderTopBar,
 * ReaderRail) kept its own local `theme` $state initialized from getTheme(),
 * so toggling via one mounted instance left every other simultaneously-
 * mounted instance (e.g. the desktop button, hidden by CSS but still in the
 * DOM at mobile widths) showing a stale icon until a remount.
 */
class ThemeState {
	current = $state<Theme>('light');

	load() {
		this.current = getTheme();
	}

	toggle() {
		this.current = this.current === 'dark' ? 'light' : 'dark';
		setTheme(this.current);
	}
}

export const themeState = new ThemeState();
