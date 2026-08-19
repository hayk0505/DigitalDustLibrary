// Same above/below-fit + viewport-clamp algorithm MobileTurntablePlayer.svelte
// uses for its own floating panel, factored out so the two reader-mode
// panels (text settings, contents) don't each reimplement it — those two
// don't need that component's drag-tracking, just "position relative to a
// static trigger button, flipped and clamped to stay on screen."
export type PanelPosition = { top: number; left: number };

const EDGE_MARGIN = 8;
const GAP = 10;

export function positionAnchoredPanel(
	triggerEl: HTMLElement,
	panelEl: HTMLElement,
	panelWidth: number
): PanelPosition {
	const triggerRect = triggerEl.getBoundingClientRect();
	// offsetHeight, not getBoundingClientRect().height — see
	// MobileTurntablePlayer's positionPanel for why: this can run while the
	// panel is still mid open-transition, and offsetHeight ignores that
	// transform entirely.
	const panelHeight = panelEl.offsetHeight;

	const spaceBelow = window.innerHeight - triggerRect.bottom;
	const fitsBelow = spaceBelow >= panelHeight + GAP;
	let top = fitsBelow ? triggerRect.bottom + GAP : triggerRect.top - GAP - panelHeight;
	top = Math.min(Math.max(top, EDGE_MARGIN), window.innerHeight - panelHeight - EDGE_MARGIN);

	const left = Math.min(
		Math.max(triggerRect.left, EDGE_MARGIN),
		window.innerWidth - panelWidth - EDGE_MARGIN
	);

	return { top, left };
}

// Desktop rail panels (Contents, Text) dock flush against the rail's own
// edge instead of floating near the trigger — a fixed left (the rail's own
// width) and top pinned to the trigger's own top, no gap. Mobile keeps
// positionAnchoredPanel's floating-popover behavior above; there's no rail
// edge there to dock against.
export function positionDockedPanel(triggerEl: HTMLElement, dockLeft: number): PanelPosition {
	const triggerRect = triggerEl.getBoundingClientRect();
	return { top: triggerRect.top, left: dockLeft };
}
