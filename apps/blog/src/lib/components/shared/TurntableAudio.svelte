<script lang="ts">
	// The one <audio> element for the whole site — see turntable.svelte.ts for
	// why playback state lives in a shared module rather than per-component.
	import { turntable } from '$lib/state/turntable.svelte';
</script>

{#if turntable.current}
	<audio
		bind:this={turntable.audio}
		src={turntable.current.src}
		preload="metadata"
		bind:currentTime={turntable.currentTime}
		bind:duration={turntable.duration}
		bind:paused={turntable.paused}
		bind:volume={turntable.volume}
		bind:muted={turntable.muted}
		onloadedmetadata={() => (turntable.unavailable = false)}
		onerror={() => (turntable.unavailable = true)}
		onended={() => turntable.goTo(turntable.index + 1, true)}
	></audio>
{/if}
