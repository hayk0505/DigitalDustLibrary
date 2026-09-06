<script lang="ts">
	import logo from '$lib/assets/logo.svg';
	import ThemeToggleButton from './ThemeToggleButton.svelte';
	import MobileMenuButton from './MobileMenuButton.svelte';
	import SavedArticlesLink from './SavedArticlesLink.svelte';
	import SearchControl from '$lib/components/search/SearchControl.svelte';
</script>

<div class="border-b border-ink/10">
	<div
		class="hidden items-center justify-between px-6 py-3 font-label text-xs tracking-widest uppercase md:flex sm:px-10"
	>
		<div class="flex items-center gap-6">
			<a href="/" class="flex items-center leading-tight" aria-label="Digital Dust Library — home">
				<img src={logo} alt="" class="h-8 w-8" />
			</a>
		</div>
		<div class="flex items-center gap-4">
			<SearchControl />
			<ThemeToggleButton />
			<SavedArticlesLink />
		</div>
	</div>

	<div class="flex items-center justify-between px-4 py-3 md:hidden">
		<a href="/" class="flex items-center leading-tight" aria-label="Digital Dust Library — home">
			<img src={logo} alt="" class="h-8 w-8" />
		</a>
		<div class="flex items-center gap-4">
			<SearchControl />
			<ThemeToggleButton iconClass="h-5 w-5" />
			<SavedArticlesLink iconClass="h-5 w-5" />
			<!-- <MobileMenuButton iconClass="h-6 w-6" /> -->
		</div>
	</div>
</div>
