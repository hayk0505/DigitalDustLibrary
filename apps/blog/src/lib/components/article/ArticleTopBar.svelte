<script lang="ts">
	import { BookOpen } from 'lucide-svelte';
	import ReadingProgressBar from './ReadingProgressBar.svelte';
	import ThemeToggleButton from '$lib/components/layout/ThemeToggleButton.svelte';
	import MobileMenuButton from '$lib/components/layout/MobileMenuButton.svelte';
	import SearchControl from '$lib/components/search/SearchControl.svelte';
	import logo from '$lib/assets/logo.svg';
	import { readerMode } from '$lib/state/reader-mode.svelte';
</script>

<div class="sticky top-0 z-20 bg-paper">
	<div class="border-b border-ink/10">
		<div class="mx-auto flex items-center justify-between py-2">
			<div class="flex items-center gap-4">
				<a href="/" class="flex items-center gap-3 leading-tight">
					<img src={logo} alt="" class="h-8 w-8" />
				</a>
				<nav class="hidden items-center gap-4 font-label text-xs tracking-widest uppercase md:flex">
					<a href="/become-an-author" class="underline hover:text-accent-red">Become an author</a>
				</nav>
			</div>
			<div class="flex items-center gap-4">
				<button
					type="button"
					onclick={() => readerMode.enter()}
					class="hidden items-center gap-1.5 font-label text-xs tracking-widest uppercase hover:text-accent-red md:flex"
				>
					<BookOpen class="h-3.5 w-3.5" aria-hidden="true" />
					Reader Mode
				</button>
				<ThemeToggleButton class="hidden text-ink/70 hover:text-ink md:flex" />
				<SearchControl />
				<button
					type="button"
					onclick={() => readerMode.enter()}
					aria-label="Reader Mode"
					title="Reader Mode"
					class="flex text-ink/70 hover:text-ink md:hidden"
				>
					<BookOpen class="h-5 w-5" />
				</button>
				<ThemeToggleButton class="flex text-ink/70 hover:text-ink md:hidden" iconClass="h-5 w-5" />
				<MobileMenuButton class="text-ink/70 hover:text-ink md:hidden" />
			</div>
		</div>
	</div>
	<ReadingProgressBar />
</div>
