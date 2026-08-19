// Shared by ReadingProgressBar (normal article chrome) and the reader-mode
// nav bars (desktop and mobile both show a "N% read" figure) — extracted
// so all three track scroll position the same way instead of each running
// its own listener.
export function createReadingProgress() {
	let progress = $state(0);

	$effect(() => {
		let ticking = false;

		function update() {
			const scrollable = document.documentElement.scrollHeight - window.innerHeight;
			progress = scrollable > 0 ? Math.min(100, Math.max(0, (window.scrollY / scrollable) * 100)) : 0;
		}

		function onScroll() {
			if (ticking) return;
			ticking = true;
			requestAnimationFrame(() => {
				update();
				ticking = false;
			});
		}

		update();
		window.addEventListener('scroll', onScroll, { passive: true });
		window.addEventListener('resize', update);
		return () => {
			window.removeEventListener('scroll', onScroll);
			window.removeEventListener('resize', update);
		};
	});

	return {
		get value() {
			return progress;
		}
	};
}
