<script lang="ts">
	let progress = $state(0);

	function updateProgress() {
		const scrollable = document.documentElement.scrollHeight - window.innerHeight;
		progress = scrollable > 0 ? Math.min(100, Math.max(0, (window.scrollY / scrollable) * 100)) : 0;
	}

	$effect(() => {
		let ticking = false;
		function onScroll() {
			if (ticking) return;
			ticking = true;
			requestAnimationFrame(() => {
				updateProgress();
				ticking = false;
			});
		}

		updateProgress();
		window.addEventListener('scroll', onScroll, { passive: true });
		window.addEventListener('resize', updateProgress);
		return () => {
			window.removeEventListener('scroll', onScroll);
			window.removeEventListener('resize', updateProgress);
		};
	});
</script>

<div class="h-[3px] w-full bg-ink/10">
	<div
		class="h-full bg-accent-red transition-[width] duration-150 ease-out"
		style="width: {progress}%"
	></div>
</div>
