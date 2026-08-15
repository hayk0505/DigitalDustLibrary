<script lang="ts">
	import logo from '$lib/assets/logo.svg';
</script>

<header class="border-ink/10 bg-paper">
	<div class="mx-auto flex max-w-6xl items-center justify-between px-6 py-4">
		<a href="/" class="flex items-center gap-3 leading-tight mr-5">
			<img src={logo} alt="" class="h-8 w-8" />
		</a>
		<nav class="flex items-center gap-6 font-label text-xs tracking-widest uppercase">
			<a href="/archive" class="underline hover:text-accent-red">Archive</a>
			<a href="/become-an-author" class="underline hover:text-accent-red">Become an author</a>
		</nav>
	</div>
</header>
