<script lang="ts">
	import logo from '$lib/assets/logo.svg';
	import { ADMIN_URL } from '$lib/config';
</script>

<header class="border-b border-ink/10 bg-paper">
	<div class="mx-auto flex max-w-6xl items-center justify-between px-6 py-4">
		<a href="/" class="flex items-center gap-3 leading-tight">
			<img src={logo} alt="" class="h-8 w-8" />
			<span>
				<span class="font-brand text-[26px] leading-[0.95] font-black tracking-[-0.02em] uppercase">
					Digital Dust <span class="text-accent-red">Library</span>
				</span>
				<p class="font-label text-[10px] tracking-widest text-ink/50 uppercase">
					Field notes on what the internet leaves behind
				</p>
			</span>
		</a>
		<nav class="flex items-center gap-6 font-label text-xs tracking-widest uppercase">
			<a href="/archive" class="hover:text-accent-red">Archive</a>
			<a href="/become-an-author" class="hover:text-accent-red">Become an author</a>
			<a
				href={ADMIN_URL}
				class="rounded-none border border-ink px-3 py-1.5 hover:bg-ink hover:text-paper"
			>
				Log in
			</a>
		</nav>
	</div>
</header>
