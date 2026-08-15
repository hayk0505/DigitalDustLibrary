<script lang="ts">
	import { Menu } from 'lucide-svelte';
	import ShareLinks from './ShareLinks.svelte';
	import ReadingProgressBar from './ReadingProgressBar.svelte';
	import MobileMenuDrawer from '$lib/components/layout/MobileMenuDrawer.svelte';
	import logo from '$lib/assets/logo.svg';

	let { shareUrl }: { shareUrl: string } = $props();

	let drawerOpen = $state(false);
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
				<ShareLinks url={shareUrl} />
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
