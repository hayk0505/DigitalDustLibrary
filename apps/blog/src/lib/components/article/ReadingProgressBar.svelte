<script lang="ts">
	import { createReadingProgress } from '$lib/utils/reading-progress.svelte';

	const progress = createReadingProgress();
</script>

<div class="h-[3px] w-full bg-ink/10">
	<div
		class="h-full bg-accent-red transition-[width] duration-150 ease-out"
		style="width: {progress.value}%"
	></div>
</div>
