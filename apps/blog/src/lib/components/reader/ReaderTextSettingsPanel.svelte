<script lang="ts">
	import type { Snippet } from 'svelte';
	import { X } from 'lucide-svelte';
	import { positionAnchoredPanel, positionDockedPanel } from '$lib/utils/anchor-panel';
	import {
		readerMode,
		FONT_SIZE_MIN,
		FONT_SIZE_MAX,
		LINE_HEIGHT_MIN,
		LINE_HEIGHT_MAX,
		READING_WIDTH_MIN,
		READING_WIDTH_MAX
	} from '$lib/state/reader-mode.svelte';

	let {
		class: triggerClass = '',
		children,
		dockLeft
	}: { class?: string; children?: Snippet; dockLeft?: number } = $props();

	const PANEL_WIDTH = 300;

	// Drives the filled portion of the custom slider track (app.css) via a
	// CSS custom property — WebKit/Chromium have no equivalent of Firefox's
	// ::-moz-range-progress, so the fill has to be painted as a
	// value-dependent gradient on the track itself instead.
	function percent(value: number, min: number, max: number): number {
		return ((value - min) / (max - min)) * 100;
	}

	let open = $state(false);
	let triggerEl = $state<HTMLButtonElement | null>(null);
	let panelEl = $state<HTMLDivElement | null>(null);
	let top = $state(0);
	let left = $state(0);

	function reposition() {
		if (!triggerEl || !panelEl) return;
		({ top, left } =
			dockLeft !== undefined
				? positionDockedPanel(triggerEl, dockLeft)
				: positionAnchoredPanel(triggerEl, panelEl, PANEL_WIDTH));
	}

	// Reruns whenever the panel opens/mounts — a first pass runs the moment
	// it appears, using its real measured height. Same pattern as
	// MobileTurntablePlayer's own positioning effect.
	$effect(() => {
		if (open && triggerEl && panelEl) reposition();
	});

	function close() {
		open = false;
	}

	function onKeydown(e: KeyboardEvent) {
		if (e.key === 'Escape') close();
	}

	function inputValue(e: Event): number {
		return Number((e.currentTarget as HTMLInputElement).value);
	}
</script>

<svelte:window onkeydown={open ? onKeydown : undefined} onresize={open ? reposition : undefined} />

<button
	type="button"
	bind:this={triggerEl}
	onclick={() => (open = !open)}
	class={triggerClass}
	aria-label="Text settings"
	aria-expanded={open}
	aria-haspopup="dialog"
	title="Text settings"
>
	{@render children?.()}
</button>

{#if open}
	<button
		type="button"
		class="dd-reader-panel-backdrop"
		onclick={close}
		aria-label="Close text settings"
	></button>
	<div
		class="dd-reader-panel"
		bind:this={panelEl}
		style="top: {top}px; left: {left}px; width: {PANEL_WIDTH}px;"
		role="dialog"
		aria-modal="true"
		aria-label="Text settings"
	>
		<div class="dd-reader-panel-header">
			<span>Text</span>
			<button type="button" onclick={close} aria-label="Close text settings">
				<X class="h-3.5 w-3.5" aria-hidden="true" />
			</button>
		</div>

		<!-- A shared grid, not each row sizing its own label/slider split —
		     with three different label lengths ("Size" vs "Line height"),
		     independent flex rows give each slider a different leftover
		     width. Grid-aligning them under one container makes the label
		     column as wide as the widest label (auto-sizes, not a guessed
		     px value) and the slider column an equal 1fr in every row. -->
		<div class="dd-reader-panel-rows">
			<label class="dd-reader-panel-row">
				<span>Size</span>
				<input
					type="range"
					min={FONT_SIZE_MIN}
					max={FONT_SIZE_MAX}
					step="1"
					value={readerMode.fontSize}
					style="--range-percent: {percent(readerMode.fontSize, FONT_SIZE_MIN, FONT_SIZE_MAX)}%"
					oninput={(e) => readerMode.setFontSize(inputValue(e))}
				/>
			</label>

			<label class="dd-reader-panel-row">
				<span>Line height</span>
				<input
					type="range"
					min={LINE_HEIGHT_MIN}
					max={LINE_HEIGHT_MAX}
					step="0.05"
					value={readerMode.lineHeight}
					style="--range-percent: {percent(readerMode.lineHeight, LINE_HEIGHT_MIN, LINE_HEIGHT_MAX)}%"
					oninput={(e) => readerMode.setLineHeight(inputValue(e))}
				/>
			</label>

			<!-- Reading width only means anything on desktop's centered column —
			     mobile fills the screen edge to edge (see .dd-reader-article), so
			     there's nothing for this control to do there. A dedicated class,
			     not Tailwind's hidden/md:flex: those are layered utilities and
			     .dd-reader-panel-row is plain unlayered CSS, which always wins
			     the display property regardless of Tailwind's own breakpoint
			     class — same reason the .prose overrides above work unlayered. -->
			<label class="dd-reader-panel-row dd-reader-panel-row-desktop-only">
				<span>Width</span>
				<input
					type="range"
					min={READING_WIDTH_MIN}
					max={READING_WIDTH_MAX}
					step="10"
					value={readerMode.readingWidth}
					style="--range-percent: {percent(readerMode.readingWidth, READING_WIDTH_MIN, READING_WIDTH_MAX)}%"
					oninput={(e) => readerMode.setReadingWidth(inputValue(e))}
				/>
			</label>
		</div>
	</div>
{/if}
