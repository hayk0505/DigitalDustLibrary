<script lang="ts">
	import { scale } from 'svelte/transition';
	import { cubicOut } from 'svelte/easing';
	import { Pause, Play, SkipBack, SkipForward, Volume2, VolumeX, X } from 'lucide-svelte';
	import { turntable, formatTime } from '$lib/state/turntable.svelte';

	let expanded = $state(false);
	let fabEl = $state<HTMLButtonElement | null>(null);
	let panelEl = $state<HTMLDivElement | null>(null);

	// Panel position — computed relative to the FAB's actual on-screen rect
	// (which reflects any drag offset), not a fixed corner, so the panel
	// follows the bubble wherever it's been dragged to. Recomputed by the
	// $effect below whenever the panel (re)mounts or the FAB moves.
	let panelTop = $state(0);
	let panelLeft = $state(0);

	const PANEL_WIDTH = 284; // must match .dd-mobile-panel's CSS width
	const GAP = 10;
	const EDGE_MARGIN = 8;

	function positionPanel() {
		if (!fabEl || !panelEl) return;
		const fabRect = fabEl.getBoundingClientRect();
		// offsetHeight, not getBoundingClientRect().height — the panel opens
		// with a scale() transition, and this runs while that's still
		// animating from its 0.92 starting scale. getBoundingClientRect
		// reports the currently-rendered (shrunk) size at that instant;
		// offsetHeight is the layout size and ignores transform entirely, so
		// the below/above decision isn't skewed by catching it mid-animation.
		const panelHeight = panelEl.offsetHeight;

		const spaceBelow = window.innerHeight - fabRect.bottom;
		const fitsBelow = spaceBelow >= panelHeight + GAP;
		let top = fitsBelow ? fabRect.bottom + GAP : fabRect.top - GAP - panelHeight;
		// Safety clamp for the case neither side has room (e.g. a short
		// landscape viewport) — keeps the panel on-screen rather than trusting
		// the below/above choice alone.
		top = Math.min(Math.max(top, EDGE_MARGIN), window.innerHeight - panelHeight - EDGE_MARGIN);
		panelTop = top;

		// Right-aligned with the FAB by default (it usually sits near the
		// screen's right edge), clamped since dragging can put it anywhere.
		const rawLeft = fabRect.right - PANEL_WIDTH;
		panelLeft = Math.min(
			Math.max(rawLeft, EDGE_MARGIN),
			window.innerWidth - PANEL_WIDTH - EDGE_MARGIN
		);
	}

	// Reruns whenever the panel mounts/unmounts (bind:this flips panelEl)
	// or the FAB moves — a first pass runs the moment the panel appears,
	// using its real measured height rather than an estimate.
	$effect(() => {
		if (expanded && fabEl && panelEl) positionPanel();
	});

	// Collapsed-bubble dragging. dragOffset feeds both the FAB's own transform
	// and, via positionPanel() above, the panel's position — so the panel
	// tracks the bubble as it's dragged, not just at the moment it opens.
	let dragOffset = $state({ x: 0, y: 0 });
	let dragging = $state(false);

	// Plain (non-reactive) scratch state for the gesture itself — nothing
	// here drives a render, only the commits to dragOffset above do.
	let dragStartPointer = { x: 0, y: 0 };
	let dragStartOffset = { x: 0, y: 0 };
	let dragBounds = { minX: 0, maxX: 0, minY: 0, maxY: 0 };
	let dragMoved = false;

	// Below this, a pointerdown+up is a tap, not a drag — keeps a light touch
	// from being swallowed as an accidental drag.
	const DRAG_THRESHOLD = 6;

	function onFabPointerDown(e: PointerEvent) {
		if (e.button !== 0) return;
		const el = e.currentTarget as HTMLButtonElement;
		const rect = el.getBoundingClientRect();
		// The rect already reflects any offset from a previous drag, so
		// subtracting it back out gives the button's un-dragged (CSS-anchored)
		// position — the fixed reference every subsequent clamp is measured
		// against, recomputed fresh each drag in case the viewport was resized.
		const baseLeft = rect.left - dragOffset.x;
		const baseTop = rect.top - dragOffset.y;
		dragBounds = {
			minX: -baseLeft,
			maxX: window.innerWidth - rect.width - baseLeft,
			minY: -baseTop,
			maxY: window.innerHeight - rect.height - baseTop
		};
		dragStartPointer = { x: e.clientX, y: e.clientY };
		dragStartOffset = { ...dragOffset };
		dragMoved = false;
		dragging = true;
		el.setPointerCapture(e.pointerId);
	}

	function onFabPointerMove(e: PointerEvent) {
		if (!dragging) return;
		const dx = e.clientX - dragStartPointer.x;
		const dy = e.clientY - dragStartPointer.y;
		if (!dragMoved && Math.hypot(dx, dy) > DRAG_THRESHOLD) dragMoved = true;
		if (!dragMoved) return;
		dragOffset = {
			x: Math.min(Math.max(dragStartOffset.x + dx, dragBounds.minX), dragBounds.maxX),
			y: Math.min(Math.max(dragStartOffset.y + dy, dragBounds.minY), dragBounds.maxY)
		};
		// Keeps the panel following the bubble live if it's already open while
		// being dragged, rather than only snapping to the new spot on release.
		if (expanded) positionPanel();
	}

	function onFabPointerUp() {
		dragging = false;
	}

	function onFabClick() {
		// A drag ends with the same click event a tap would fire — consume it
		// here rather than toggling the panel the drag didn't intend to trigger.
		if (dragMoved) {
			dragMoved = false;
			return;
		}
		// A toggle, not just "open" — the bubble now stays visible and
		// clickable even while the panel is already showing.
		expanded = !expanded;
	}

	function onWindowResize() {
		if (expanded) positionPanel();
	}

	// Tonearm angles in degrees, solved for THIS disc's own geometry (pivot
	// 86,20; record centre 42,56; arm length 51.42) — independent of the
	// desktop player's constants, which are solved for a differently
	// positioned, cropped composition. 0 lands the stylus on the outer groove
	// (r=34.9), ARM_END puts it at the run-out beside the label (r=18), and
	// ARM_PARKED lifts it clear of the disc entirely (r=41.6 against a radius
	// of 38, with the whole arm — not just the tip — verified clear).
	const ARM_PARKED = -7.6;
	const ARM_END = 19.0;
	const armAngle = $derived(turntable.armEngaged ? ARM_END * turntable.fraction : ARM_PARKED);

	const grooves = [34, 31, 28, 25, 22, 19, 16];
	const RING = 2 * Math.PI * 38;

	// Collapsed-bubble-only geometry, own viewBox (0 0 80 80) rather than a
	// scaled-down version of the panel's — the tonearm reads as noise at
	// ~58px so it's dropped here in favour of a static "signal pin" accent,
	// and the freed-up space is used for the wave-arc rings instead. Disc
	// centre (52,52) r=22, offset toward the bottom-right so those two
	// accents have room in the top-left/top-right corners.
	const fabGrooves = [19, 17, 15, 13, 11];

	// Artist name on the label — only for the expanded panel's 128px disc.
	// The collapsed bubble's disc renders at ~15px across (label radius 14 in
	// a 100-unit viewBox, scaled to a 52px bubble); no font size reads at
	// that size, so it stays plain there. Sized in this SVG's own units
	// (viewBox 100 = 128 rendered px, a 1.28x scale) to land around 6px
	// actual on screen — same wrap-by-character-budget approach as the
	// desktop label, re-solved for this disc's smaller r=14 (vs desktop's
	// r=28) and capped at 2 lines instead of 3, since a 3rd line doesn't
	// clear the smaller circle with any margin to spare.
	const LABEL_CHARS = 8;
	const LABEL_LINES = 2;
	const LABEL_LINE_HEIGHT = 5.9;
	const LABEL_BASELINE_OFFSET = 1.68;

	function wrapLabel(text: string): string[] {
		const lines: string[] = [];
		let line = '';

		for (const word of text.trim().toUpperCase().split(/\s+/)) {
			if (!word) continue;
			const candidate = line ? `${line} ${word}` : word;
			// `!line` keeps a single over-long word rather than dropping it.
			if (candidate.length <= LABEL_CHARS || !line) {
				line = candidate;
			} else {
				lines.push(line);
				line = word;
				if (lines.length === LABEL_LINES) break;
			}
		}
		if (line && lines.length < LABEL_LINES) lines.push(line);

		return lines
			.slice(0, LABEL_LINES)
			.map((l) => (l.length > LABEL_CHARS ? `${l.slice(0, LABEL_CHARS - 1)}…` : l));
	}

	const labelLines = $derived(wrapLabel(turntable.current?.artist ?? ''));

	function prefersReducedMotion(): boolean {
		return (
			typeof window !== 'undefined' && window.matchMedia('(prefers-reduced-motion: reduce)').matches
		);
	}

	function panelTransitionParams() {
		return prefersReducedMotion()
			? { duration: 0 }
			: { duration: 200, easing: cubicOut, start: 0.92, opacity: 0 };
	}

	function collapse() {
		expanded = false;
	}

	function onKeydown(e: KeyboardEvent) {
		if (e.key === 'Escape') collapse();
	}
</script>

<svelte:window
	onkeydown={expanded ? onKeydown : undefined}
	onresize={expanded ? onWindowResize : undefined}
/>

{#if turntable.current}
	<div class="dd-mobile-player md:hidden">
		{#if expanded}
			<div
				class="dd-mobile-panel"
				bind:this={panelEl}
				style="top: {panelTop}px; left: {panelLeft}px;"
				transition:scale={panelTransitionParams()}
			>
				<!-- A real, focusable <button> covering the whole panel, stacked
				     BEHIND the visible content via z-index — not a wrapping div,
				     since the four control buttons below must stay siblings, not
				     descendants, of whatever handles "tap background to collapse"
				     (nesting interactive roles is invalid ARIA). Being a real
				     <button> also means Enter/Space work with no extra key
				     handling; the window-level Escape listener above is the other
				     dismiss path. -->
				<button
					type="button"
					class="dd-mobile-backdrop"
					onclick={collapse}
					aria-label="Collapse turntable player"
				></button>

				<!-- Explicit stacking layer above the backdrop — needed because a
				     couple of these children (the chip) aren't themselves
				     position:absolute, so they wouldn't reliably paint above an
				     absolutely-positioned sibling without one. -->
				<div class="dd-mobile-panel-content">
					<div class="dd-mobile-header">
						<div class="dd-mobile-chip">
							<span
								class="dd-mobile-chip-eq"
								class:is-live={turntable.isPlaying}
								aria-hidden="true"
							>
								{#each [5, 9, 6, 10, 4] as height, i (i)}
									<i style="height: {height}px"></i>
								{/each}
							</span>
							<span class="dd-mobile-chip-text">
								<span class="dd-mobile-chip-label">Now Playing</span>
								<span class="dd-mobile-chip-title">{turntable.current.title}</span>
							</span>
						</div>

						<button
							type="button"
							class="dd-mobile-close"
							onclick={collapse}
							aria-label="Hide turntable player"
							title="Hide turntable player"
						>
							<X class="h-3.5 w-3.5" aria-hidden="true" />
						</button>
					</div>
					<p class="sr-only">{turntable.current.artist}</p>

					<div class="dd-mobile-body">
						<div class="dd-mobile-disc-wrap dd-mobile-disc-lg" aria-hidden="true">
							<svg viewBox="0 0 100 100" fill="none" xmlns="http://www.w3.org/2000/svg">
								<g class="dd-mobile-disc" class:is-spinning={turntable.isPlaying}>
									<circle cx="42" cy="56" r="38" class="dd-vinyl" />
									{#each grooves as r (r)}
										<circle cx="42" cy="56" {r} class="dd-groove" />
									{/each}
									<path d="M 6.3 43.0 A 38 38 0 0 1 55.0 20.3" class="dd-sheen" />
									<circle
										cx="42"
										cy="56"
										r="14"
										class="dd-label"
										style:fill={turntable.current.labelColor}
									/>
									<circle cx="42" cy="56" r="14" class="dd-label-edge" />
									<circle cx="42" cy="56" r="1.6" class="dd-spindle" />
									{#if labelLines.length > 0}
										<text class="dd-mobile-label-text" x="42" y="56" text-anchor="middle">
											{#each labelLines as line, i (i)}
												<tspan
													x="42"
													dy={i === 0
														? LABEL_BASELINE_OFFSET -
															((labelLines.length - 1) * LABEL_LINE_HEIGHT) / 2
														: LABEL_LINE_HEIGHT}>{line}</tspan
												>
											{/each}
										</text>
									{/if}
								</g>

								<circle cx="42" cy="56" r="38" class="dd-ring-track" />
								<circle
									cx="42"
									cy="56"
									r="38"
									class="dd-ring-fill"
									style="stroke-dasharray: {RING}; stroke-dashoffset: {RING *
										(1 - turntable.fraction)}; transform-origin: 42px 56px"
								/>

								<rect x="78" y="12" width="16" height="16" rx="3" class="dd-arm-plinth" />

								<g class="dd-mobile-tonearm" style="transform: rotate({armAngle}deg)">
									<line x1="86" y1="20" x2="88.3" y2="10.3" class="dd-arm" />
									<rect x="84.5" y="6.8" width="7" height="7" rx="1.5" class="dd-arm-weight" />
									<line x1="86" y1="20" x2="74" y2="70" class="dd-arm" />
									<rect x="70" y="66" width="8" height="8" rx="1.5" class="dd-arm-head" />
									<circle cx="86" cy="20" r="4.5" class="dd-arm-pivot" />
									<circle cx="86" cy="20" r="1.6" class="dd-arm-pivot-dot" />
								</g>
							</svg>
						</div>

						<!-- prev+next as a top pair, pause below as the big primary
						     control — mute lives with the volume slider instead. -->
						<div class="dd-mobile-stack">
							<div class="dd-mobile-stack-row">
								<button
									type="button"
									class="dd-mobile-ctrl"
									onclick={() => turntable.prev()}
									aria-label="Previous track"
									title="Previous track"
								>
									<SkipBack class="h-4 w-4" aria-hidden="true" />
								</button>

								<button
									type="button"
									class="dd-mobile-ctrl"
									onclick={() => turntable.next()}
									aria-label="Next track"
									title="Next track"
								>
									<SkipForward class="h-4 w-4" aria-hidden="true" />
								</button>
							</div>

							<button
								type="button"
								class="dd-mobile-ctrl dd-mobile-ctrl-play"
								onclick={() => turntable.togglePlay()}
								aria-label={turntable.isPlaying ? 'Pause' : 'Play'}
								title={turntable.isPlaying ? 'Pause' : 'Play'}
							>
								{#if turntable.isPlaying}
									<Pause class="h-5 w-5" aria-hidden="true" />
								{:else}
									<Play class="h-5 w-5" aria-hidden="true" />
								{/if}
							</button>
						</div>
					</div>

					<div class="dd-mobile-volume">
						<button
							type="button"
							class="dd-mobile-ctrl"
							onclick={() => turntable.toggleMute()}
							aria-label={turntable.isSilent ? 'Unmute' : 'Mute'}
							title={turntable.isSilent ? 'Unmute' : 'Mute'}
						>
							{#if turntable.isSilent}
								<VolumeX class="h-4 w-4" aria-hidden="true" />
							{:else}
								<Volume2 class="h-4 w-4" aria-hidden="true" />
							{/if}
						</button>

						<input
							type="range"
							class="dd-volume-slider dd-volume-slider-mobile"
							min="0"
							max="1"
							step="0.01"
							value={turntable.volume}
							oninput={(e) => turntable.setVolume(+e.currentTarget.value)}
							aria-label="Volume"
							title="Volume"
						/>
					</div>

					{#if !turntable.unavailable}
						<p class="dd-mobile-time">
							{formatTime(turntable.currentTime)} / {formatTime(turntable.duration)}
						</p>
					{:else}
						<p class="dd-mobile-time dd-mobile-unavailable">Audio unavailable</p>
					{/if}
				</div>
			</div>
		{/if}

		<!-- Rendered regardless of `expanded` — the bubble stays visible and
		     clickable (toggling the panel) even while the panel is open. -->
		<button
			type="button"
			class="dd-mobile-fab"
			class:is-dragging={dragging}
			bind:this={fabEl}
			style="transform: translate({dragOffset.x}px, {dragOffset.y}px)"
			onpointerdown={onFabPointerDown}
			onpointermove={onFabPointerMove}
			onpointerup={onFabPointerUp}
			onpointercancel={onFabPointerUp}
			onclick={onFabClick}
			aria-label={expanded
				? 'Close turntable player'
				: `Open turntable player — now playing ${turntable.current.title}`}
			aria-expanded={expanded}
			title={expanded ? 'Close turntable player' : 'Open turntable player'}
		>
			<div class="dd-mobile-disc-wrap dd-mobile-disc-sm" aria-hidden="true">
				<svg viewBox="0 0 80 80" fill="none" xmlns="http://www.w3.org/2000/svg">
					<g class="dd-mobile-fab-waves" class:is-live={turntable.isPlaying}>
						<path d="M 24.4 47.1 A 28 28 0 0 1 47.1 24.4" class="dd-mobile-fab-wave" />
						<path d="M 18.5 46.1 A 34 34 0 0 1 46.1 18.5" class="dd-mobile-fab-wave" />
						<path d="M 12.6 45.0 A 40 40 0 0 1 45.1 12.6" class="dd-mobile-fab-wave" />
					</g>

					<g
						class="dd-mobile-disc"
						class:is-spinning={turntable.isPlaying}
						style="transform-origin: 52px 52px;"
					>
						<circle cx="52" cy="52" r="22" class="dd-vinyl" />
						{#each fabGrooves as r (r)}
							<circle cx="52" cy="52" {r} class="dd-groove" />
						{/each}
						<path d="M 31.3 44.5 A 22 22 0 0 1 59.5 31.3" class="dd-sheen" />
						<circle
							cx="52"
							cy="52"
							r="8"
							class="dd-label"
							style:fill={turntable.current.labelColor}
						/>
						<circle cx="52" cy="52" r="8" class="dd-label-edge" />
						<circle cx="52" cy="52" r="1.2" class="dd-spindle" />
					</g>

					<!-- Static "signal pin" accent, not a functional tonearm — see the
					     fabGrooves comment above for why. -->
					<line x1="65" y1="36" x2="73" y2="19" class="dd-arm" />
					<rect x="70" y="16" width="6" height="6" rx="1.5" class="dd-arm-head" />
				</svg>
			</div>
		</button>
	</div>
{/if}
