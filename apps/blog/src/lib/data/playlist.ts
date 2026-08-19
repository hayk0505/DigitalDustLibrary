export type { Track } from './types';

/**
 * The sidebar turntable's playlist used to be generated at build time by
 * scanning apps/blog/static/audio/ (see the now-deleted
 * vite-plugins/audio-playlist.ts). That required every track to be committed
 * to this repo, which doesn't scale for multi-megabyte audio files. Tracks
 * now live only on the droplet (see docs/deployment.md's audio-files
 * section) and are fetched at request time via $lib/api.ts's fetchPlaylist,
 * wired up in (site)/+layout.ts and pushed into $lib/state/turntable.svelte's
 * reactive state client-side. Nothing here needs editing to add a track —
 * scp it onto the droplet named "Artist - Title.ext" and it appears on the
 * next page load, same ergonomics as before.
 */
