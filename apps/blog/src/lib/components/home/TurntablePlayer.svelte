<script lang="ts">
	import { Pause, Play, SkipBack, SkipForward, Volume2, VolumeX } from 'lucide-svelte';
	import { turntable, formatTime } from '$lib/state/turntable.svelte';

	const grooves = [73, 69, 65, 60, 55, 50, 45, 39, 33];

	const ARM_PARKED = -8.93;
	const ARM_END = 23.94;
	const armAngle = $derived(turntable.armEngaged ? ARM_END * turntable.fraction : ARM_PARKED);

	const RING = 2 * Math.PI * 78;

	const LABEL_CHARS = 8;
	const LABEL_LINES = 3;
	const LINE_HEIGHT = 10;

	function wrapLabel(text: string): string[] {
		const lines: string[] = [];
		let line = '';

		for (const word of text.trim().toUpperCase().split(/\s+/)) {
			if (!word) continue;
			const candidate = line ? `${line} ${word}` : word;
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
</script>

{#if turntable.current}
	<section class="dd-player" aria-label="Turntable player">
		<div class="dd-player-panel">
			<p class="dd-player-label" class:is-live={turntable.isPlaying}>Now Playing</p>
			<h2 class="dd-player-title">{turntable.current.title}</h2>
			<p class="sr-only">{turntable.current.artist}</p>

			{#if turntable.unavailable}
				<p class="dd-player-unavailable">Audio unavailable</p>
			{:else}
				<p class="dd-player-time">
					{formatTime(turntable.currentTime)} / {formatTime(turntable.duration)}
				</p>
			{/if}

			<div class="dd-controls">
				<button
					type="button"
					class="dd-btn"
					onclick={() => turntable.prev()}
					aria-label="Previous track"
					title="Previous track"
				>
					<SkipBack class="h-3.5 w-3.5" aria-hidden="true" />
				</button>

				<button
					type="button"
					class="dd-btn dd-btn-play"
					onclick={() => turntable.togglePlay()}
					aria-label={turntable.isPlaying ? 'Pause' : 'Play'}
					title={turntable.isPlaying ? 'Pause' : 'Play'}
				>
					{#if turntable.isPlaying}
						<Pause class="h-4 w-4" aria-hidden="true" />
					{:else}
						<Play class="h-4 w-4" aria-hidden="true" />
					{/if}
				</button>

				<button
					type="button"
					class="dd-btn"
					onclick={() => turntable.next()}
					aria-label="Next track"
					title="Next track"
				>
					<SkipForward class="h-3.5 w-3.5" aria-hidden="true" />
				</button>
			</div>

			<div class="dd-volume">
				<button
					type="button"
					class="dd-btn"
					onclick={() => turntable.toggleMute()}
					aria-label={turntable.isSilent ? 'Unmute' : 'Mute'}
					title={turntable.isSilent ? 'Unmute' : 'Mute'}
				>
					{#if turntable.isSilent}
						<VolumeX class="h-3.5 w-3.5" aria-hidden="true" />
					{:else}
						<Volume2 class="h-3.5 w-3.5" aria-hidden="true" />
					{/if}
				</button>

				<input
					type="range"
					class="dd-volume-slider"
					min="0"
					max="1"
					step="0.01"
					value={turntable.volume}
					oninput={(e) => turntable.setVolume(+e.currentTarget.value)}
					aria-label="Volume"
					title="Volume"
				/>
			</div>
		</div>

		<div class="dd-turntable" aria-hidden="true">
			<svg viewBox="0 0 142 124" fill="none" xmlns="http://www.w3.org/2000/svg">
				<g class="dd-disc" class:is-spinning={turntable.isPlaying}>
					<circle cx="40" cy="92" r="78" class="dd-vinyl" />
					{#each grooves as r (r)}
						<circle cx="40" cy="92" {r} class="dd-groove" />
					{/each}
					<path d="M -33.3 65.3 A 78 78 0 0 1 66.7 18.7" class="dd-sheen" />

					<circle
						cx="40"
						cy="92"
						r="28"
						class="dd-label"
						style:fill={turntable.current.labelColor}
					/>
					<circle cx="40" cy="92" r="28" class="dd-label-edge" />
					<circle cx="40" cy="92" r="2" class="dd-spindle" />
					{#if labelLines.length > 0}
						<text class="dd-label-text" x="40" y="92" text-anchor="middle">
							{#each labelLines as line, i (i)}
								<tspan
									x="40"
									dy={i === 0 ? 3 - ((labelLines.length - 1) * LINE_HEIGHT) / 2 : LINE_HEIGHT}
									>{line}</tspan
								>
							{/each}
						</text>
					{/if}
				</g>

				<circle cx="40" cy="92" r="78" class="dd-ring-track" />
				<circle
					cx="40"
					cy="92"
					r="78"
					class="dd-ring-fill"
					style="stroke-dasharray: {RING}; stroke-dashoffset: {RING * (1 - turntable.fraction)}"
				/>

				<rect x="112" y="12" width="20" height="20" rx="3" class="dd-arm-plinth" />

				<g class="dd-tonearm" style="transform: rotate({armAngle}deg)">
					<line x1="122" y1="22" x2="124" y2="8.1" class="dd-arm" />
					<rect x="118" y="3" width="11" height="11" rx="2" class="dd-arm-weight" />
					<line x1="122" y1="22" x2="108.8" y2="113.1" class="dd-arm" />
					<rect
						x="104.3"
						y="107.6"
						width="9"
						height="11"
						rx="1.5"
						transform="rotate(8.23 108.8 113.1)"
						class="dd-arm-head"
					/>
					<circle cx="122" cy="22" r="5.5" class="dd-arm-pivot" />
					<circle cx="122" cy="22" r="2" class="dd-arm-pivot-dot" />
				</g>
			</svg>
		</div>
	</section>
{/if}
