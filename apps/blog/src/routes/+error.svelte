<script lang="ts">
	import { page } from '$app/state';
</script>

<div class="flex min-h-screen flex-col items-center justify-center gap-3 bg-paper px-6 text-center">
	<p class="font-label text-xs tracking-widest text-ink/50 uppercase">Error {page.status}</p>
	<h1 class="font-display text-3xl font-bold">
		{page.error?.message ?? 'Something went missing'}
	</h1>
	<a href="/" class="font-label text-xs tracking-widest text-accent-red uppercase hover:underline">
		← Back to all columns
	</a>
</div>
