import { tick } from 'svelte';
import type { Track } from '$lib/data/playlist';

/**
 * Single shared playback engine for the sidebar's docked player and the
 * mobile floating player. There is exactly one `<audio>` element for the
 * whole site (mounted once by `TurntableAudio.svelte`) — both UIs are pure
 * views over this state, so switching between the desktop and mobile layouts
 * (or having both mounted at once, since the desktop rail is merely
 * `display:none` below `md`, not unmounted) never creates a second audio
 * stream or lets the two views drift out of sync.
 *
 * Safe as ordinary mutable module state even though this app SSRs on
 * Cloudflare Workers (where a module can be reused across requests from
 * different users within one isolate): every mutating method here is only
 * ever called from a client-side event handler, never during a render pass,
 * so the server-side copy's fields never change from their initial values —
 * there is no path for one user's playback state to leak into another's SSR
 * output. setTracks is the one exception worth calling out explicitly: it's
 * populated from (site)/+layout.svelte inside an $effect, which — like every
 * Svelte 5 effect — only ever runs client-side after hydration, never during
 * SSR, so it holds to the same rule as the rest of this class despite being
 * wired up from a load()'d value rather than a click handler.
 */
class TurntablePlayerState {
	audio = $state<HTMLAudioElement | null>(null);
	index = $state(0);
	paused = $state(true);
	currentTime = $state(0);
	// NaN, not 0, until the element reports real metadata — 0 would render as a
	// confident "00:00" total, claiming a zero-length track.
	duration = $state(Number.NaN);
	unavailable = $state(false);
	volume = $state(0.5);
	muted = $state(false);

	// Web Audio routing, built once the <audio> element exists (see
	// initAudioGraph below). Not $state — nothing renders off these, they're
	// pure playback plumbing — and deliberately not exposed outside this
	// class.
	#audioContext: AudioContext | null = null;
	#gainNode: GainNode | null = null;

	// Starts empty and is filled in once the fetch in (site)/+layout.ts
	// resolves — see setTracks below. Reactive ($state, not a plain field)
	// so the player UI updates the moment tracks arrive instead of only on
	// the next unrelated re-render.
	tracks = $state<Track[]>([]);

	// Called once per load with whatever GET /api/public/audio returned.
	// Clamps index back into range in case a previous playlist was longer
	// (not expected in practice — the fetch only runs once per session in
	// this app's current usage — but cheap insurance against an
	// out-of-bounds `current` if that ever changes).
	setTracks(tracks: Track[]) {
		this.tracks = tracks;
		if (this.index >= tracks.length) this.index = 0;
	}

	current = $derived<Track | undefined>(this.tracks[this.index]);
	isPlaying = $derived(!this.paused && !this.unavailable);
	isSilent = $derived(this.muted || this.volume === 0);
	progress = $derived(
		this.duration > 0 ? Math.min(100, (this.currentTime / this.duration) * 100) : 0
	);
	fraction = $derived(this.progress / 100);
	// Distinguishes "paused mid-track" from "stopped at the start" — a real
	// turntable's arm rests on the record when you pause it and only lifts
	// once you actually stop/reset. currentTime resets to 0 on goTo() and
	// stays 0 until playback (or a manual seek) advances it, so "paused with
	// currentTime > 0" cleanly captures the mid-track case with no extra
	// state to track.
	armEngaged = $derived(this.isPlaying || this.currentTime > 0);

	// Every track change funnels through here, so "was it playing?" is decided
	// once at the call site instead of being re-inferred from a half-updated
	// media element after the src has already reset paused to true.
	async goTo(next: number, autoplay: boolean) {
		if (this.tracks.length === 0) return;
		const target = ((next % this.tracks.length) + this.tracks.length) % this.tracks.length;

		// Re-selecting the current track (or advancing a single-track playlist)
		// leaves src untouched, so there's no load to wait on — rewind and act.
		if (target === this.index) {
			if (this.audio) this.audio.currentTime = 0;
			if (autoplay) this.play();
			return;
		}

		this.index = target;
		this.currentTime = 0;
		this.duration = Number.NaN;
		this.unavailable = false;

		if (!autoplay) return;

		// Await the flush so play() acts on the new src rather than replaying the
		// old one — `current.src` only reaches the DOM once TurntableAudio's
		// effect runs. Deliberately NOT waiting on a media event: with
		// preload="metadata" the element settles at readyState 1 and `canplay`
		// (readyState 3) may never fire, which would strand the request forever.
		// play() on a freshly-set src is fine — the browser buffers then starts.
		await tick();
		this.play();
	}

	// Called by TurntableAudio.svelte's effect once the <audio> element
	// exists. Routes it through a GainNode instead of relying on
	// HTMLMediaElement.volume: iOS Safari (and iOS Chrome, same engine)
	// silently ignores that property — it stays pinned at 1, hardware
	// buttons only, by design — but does not restrict Web Audio gain, so
	// this is the one mechanism that gives real continuous volume control on
	// every platform instead of branching iOS from the rest. Guarded so it
	// only ever runs once per element: createMediaElementSource throws if
	// called twice on the same <audio>, and the element persists across
	// track changes (only its src attribute changes — see the class comment
	// up top), so the effect firing again on an unrelated update is a no-op
	// here rather than a re-init.
	initAudioGraph(el: HTMLAudioElement) {
		if (this.#gainNode) return;
		const AudioContextCtor =
			window.AudioContext ??
			(window as unknown as { webkitAudioContext?: typeof AudioContext }).webkitAudioContext;
		if (!AudioContextCtor) return;
		this.#audioContext = new AudioContextCtor();
		const source = this.#audioContext.createMediaElementSource(el);
		this.#gainNode = this.#audioContext.createGain();
		this.#gainNode.gain.value = this.isSilent ? 0 : this.volume;
		source.connect(this.#gainNode).connect(this.#audioContext.destination);
	}

	// Single point where volume/mute state gets pushed to the actual audio
	// output — via the GainNode when the graph above exists, or the native
	// .volume property as a fallback on the vanishingly rare browser with no
	// Web Audio support at all (that fallback just inherits iOS's
	// restriction rather than working around it, same as before this fix).
	#applyGain() {
		const level = this.isSilent ? 0 : this.volume;
		if (this.#gainNode) {
			this.#gainNode.gain.value = level;
		} else if (this.audio) {
			this.audio.volume = level;
		}
	}

	setVolume(v: number) {
		this.volume = v;
		// Dragging the slider back up should always restore sound, even if
		// silence currently comes from the mute flag rather than a zero
		// level — otherwise raising it while muted looks like the slider has
		// no effect.
		if (v > 0) this.muted = false;
		this.#applyGain();
	}

	play() {
		// Must happen on a user-gesture call path (togglePlay/prev/next, or
		// goTo's autoplay from those) — browsers create a fresh AudioContext
		// suspended until resumed from one, and initAudioGraph above may run
		// before any gesture (as soon as tracks load). onended's autoplay
		// call is fine too: by the time a track ends, the context was
		// already resumed by whatever gesture started playback.
		this.#audioContext?.resume();
		this.audio?.play().catch(() => {
			this.unavailable = true;
		});
	}

	togglePlay() {
		if (!this.audio) return;
		if (this.audio.paused) this.play();
		else this.audio.pause();
	}

	prev() {
		// Standard transport behaviour: inside the first 3s "previous" means the
		// previous track, after that it means "start this one over".
		if (this.currentTime > 3) {
			if (this.audio) this.audio.currentTime = 0;
			return;
		}
		this.goTo(this.index - 1, this.isPlaying);
	}

	next() {
		this.goTo(this.index + 1, this.isPlaying);
	}

	// Lock-screen / notification media controls and hardware media-key
	// support (headphone/Bluetooth play-pause-skip buttons) — NOT volume,
	// which no web page can intercept; the OS handles that on its own
	// regardless of this API. Called once from TurntableAudio.svelte's
	// effect; the handlers close over `this` rather than any per-call
	// value, so registering them once for the page's lifetime is enough.
	registerMediaSession() {
		if (!('mediaSession' in navigator)) return;
		navigator.mediaSession.setActionHandler('play', () => this.play());
		navigator.mediaSession.setActionHandler('pause', () => this.audio?.pause());
		navigator.mediaSession.setActionHandler('previoustrack', () => this.prev());
		navigator.mediaSession.setActionHandler('nexttrack', () => this.next());
	}

	// Reruns whenever the current track or play state changes — see the
	// effect in TurntableAudio.svelte that calls this. No artwork: the
	// tracks have no cover images, only the SVG vinyl label's fill color,
	// which isn't a usable MediaMetadata image source.
	syncMediaSession() {
		if (!('mediaSession' in navigator) || !this.current) return;
		navigator.mediaSession.metadata = new MediaMetadata({
			title: this.current.title,
			artist: this.current.artist
		});
		navigator.mediaSession.playbackState = this.isPlaying ? 'playing' : 'paused';
	}

	toggleMute() {
		// Judged on the audible result (isSilent), not the muted flag alone —
		// otherwise dragging the slider to 0 and then clicking mute would flip
		// `muted` to true while leaving volume at 0, so the second click would
		// un-mute into silence and look like the button does nothing.
		if (this.isSilent) {
			this.muted = false;
			if (this.volume === 0) this.volume = 1;
		} else {
			this.muted = true;
		}
		this.#applyGain();
	}
}

export const turntable = new TurntablePlayerState();

export function formatTime(seconds: number): string {
	if (!Number.isFinite(seconds) || seconds < 0) return '--:--';
	const m = Math.floor(seconds / 60);
	const s = Math.floor(seconds % 60);
	return `${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`;
}
