<script lang="ts">
	import { BookOpen, Menu, Sun, Moon } from 'lucide-svelte';
	import ShareLinks from './ShareLinks.svelte';
	import ReadingProgressBar from './ReadingProgressBar.svelte';
	import MobileMenuDrawer from '$lib/components/layout/MobileMenuDrawer.svelte';
	import logo from '$lib/assets/logo.svg';
	import { readerMode } from '$lib/state/reader-mode.svelte';
	import { getTheme, setTheme, type Theme } from '$lib/utils/theme';

	let { shareUrl }: { shareUrl: string } = $props();

	let drawerOpen = $state(false);

	// Used by both the desktop and mobile theme buttons below — desktop's
	// used to rely on CategorySidebar's masthead toggle (.theme-toggle-spine,
	// removed) instead of having its own; both now have their own icon.
	let theme = $state<Theme>('light');

	$effect(() => {
		theme = getTheme();
	});

	function toggleTheme() {
		theme = theme === 'dark' ? 'light' : 'dark';
		setTheme(theme);
	}
</script>

<div class="sticky top-0 z-20 bg-paper">
	<div class="border-b border-ink/10">
		<div class="mx-auto flex max-w-3xl items-center justify-between  py-2">
			<div class="flex items-center gap-4">
				<a
					href="/"
					onclick={(e) => {
						if (history.length > 1) {
							e.preventDefault();
							history.back();
						}
					}}
					class="font-label text-xs tracking-widest text-ink/70 uppercase hover:text-ink"
				>
					← Back
				</a>
				<a href="/" class="flex items-center gap-3 leading-tight">
					<img src={logo} alt="" class="h-8 w-8" />
				</a>
				<nav class="hidden items-center gap-4 font-label text-xs tracking-widest uppercase md:flex">
					<a href="/archive" class="underline hover:text-accent-red">Archive</a>
					<a href="/become-an-author" class="underline hover:text-accent-red">Become an author</a>
				</nav>
			</div>
			<div class="flex items-center gap-4">
				<!-- Strips the normal blog chrome down to just the article — see
				     $lib/state/reader-mode.svelte.ts. Text+icon label here, room
				     enough on desktop; the icon-only button below is mobile's
				     equivalent, sized for a cramped top bar instead. -->
				<button
					type="button"
					onclick={() => readerMode.enter()}
					class="hidden items-center gap-1.5 font-label text-xs tracking-widest uppercase hover:text-accent-red md:flex"
				>
					<BookOpen class="h-3.5 w-3.5" aria-hidden="true" />
					Reader Mode
				</button>
				<ShareLinks url={shareUrl} />
				<button
					type="button"
					onclick={toggleTheme}
					aria-label={theme === 'dark' ? 'Switch to light theme' : 'Switch to dark theme'}
					title="Theme"
					class="hidden text-ink/70 hover:text-ink md:flex"
				>
					{#if theme === 'dark'}
						<Moon class="h-4 w-4" aria-hidden="true" />
					{:else}
						<Sun class="h-4 w-4" aria-hidden="true" />
					{/if}
				</button>
				<button
					type="button"
					onclick={() => readerMode.enter()}
					aria-label="Reader Mode"
					title="Reader Mode"
					class="flex text-ink/70 hover:text-ink md:hidden"
				>
					<BookOpen class="h-5 w-5" />
				</button>
				<button
					type="button"
					onclick={toggleTheme}
					aria-label={theme === 'dark' ? 'Switch to light theme' : 'Switch to dark theme'}
					title="Theme"
					class="flex text-ink/70 hover:text-ink md:hidden"
				>
					{#if theme === 'dark'}
						<Moon class="h-5 w-5" />
					{:else}
						<Sun class="h-5 w-5" />
					{/if}
				</button>
				<button
					type="button"
					onclick={() => (drawerOpen = true)}
					aria-label="Open menu"
					class="text-ink/70 hover:text-ink md:hidden"
				>
					<Menu class="h-5 w-5" />
				</button>
			</div>
		</div>
	</div>
	<ReadingProgressBar />
</div>

<MobileMenuDrawer bind:open={drawerOpen} />
