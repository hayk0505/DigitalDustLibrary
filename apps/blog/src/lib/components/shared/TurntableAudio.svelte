<script lang="ts">
	// The one <audio> element for the whole site — see turntable.svelte.ts for
	// why playback state lives in a shared module rather than per-component.
	import { turntable } from '$lib/state/turntable.svelte';

	// Client-only by construction, like every $effect — builds the
	// GainNode-based volume graph once the element exists. Not bind:volume/
	// bind:muted below: turntable.initAudioGraph and setVolume/toggleMute
	// own the actual audio output now, see turntable.svelte.ts for why.
	$effect(() => {
		if (turntable.audio) turntable.initAudioGraph(turntable.audio);
	});

	// Handlers only need registering once; metadata/playback-state need
	// resyncing on every track or play/pause change — see turntable.svelte.ts
	// for what each actually does.
	$effect(() => {
		turntable.registerMediaSession();
	});
	$effect(() => {
		turntable.syncMediaSession();
	});
</script>

{#if turntable.current}
	<audio
		bind:this={turntable.audio}
		src={turntable.current.src}
		preload="metadata"
		bind:currentTime={turntable.currentTime}
		bind:duration={turntable.duration}
		bind:paused={turntable.paused}
		onloadedmetadata={() => (turntable.unavailable = false)}
		onerror={() => (turntable.unavailable = true)}
		onended={() => turntable.goTo(turntable.index + 1, true)}
	></audio>
{/if}
