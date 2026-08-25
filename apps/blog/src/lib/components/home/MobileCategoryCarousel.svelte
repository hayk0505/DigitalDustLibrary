<script lang="ts">
	import { ChevronRight } from 'lucide-svelte';
	import type { Category, Post } from '$lib/data';
	import {
		getCategoryIcon,
		getCategoryTabColor,
		getCategoryTextColor
	} from '$lib/utils/category-visuals';
	import FeaturedPostCard from './FeaturedPostCard.svelte';

	let { columns }: { columns: { category: Category; posts: Post[] }[] } = $props();

	let scrollerEl: HTMLDivElement | null = $state(null);
	let cardEls: HTMLAnchorElement[] = $state([]);
	let activeIndex = $state(0);
	let depth = $state<{ scale: number; opacity: number; z: number; tx: number }[]>([]);
	let prevTxByIndex: number[] = [];

	const OVERLAP_PX = 5;

	const activeColumn = $derived(columns[activeIndex]);
	const activeAccentColor = $derived(
		activeColumn
			? (activeColumn.category.folderColor ?? getCategoryTabColor(activeColumn.category.slug))
			: ''
	);
	const activeFeaturedPost = $derived(activeColumn?.posts[0] ?? null);

	function updateDepth() {
		if (!scrollerEl) return;
		const containerRect = scrollerEl.getBoundingClientRect();
		const containerCenter = containerRect.left + containerRect.width / 2;

		let nearestIndex = 0;
		let nearestDistance = Infinity;
		const nextDepth = cardEls.map((el, i) => {
			if (!el) return { scale: 1, opacity: 1, z: 0, tx: 0 };
			const rect = el.getBoundingClientRect();
			const prevTx = prevTxByIndex[i] ?? 0;
			const cardCenter = rect.left + rect.width / 2 - prevTx;
			const signedDistance = cardCenter - containerCenter;
			const distance = Math.abs(signedDistance);
			if (distance < nearestDistance) {
				nearestDistance = distance;
				nearestIndex = i;
			}
			const fraction = Math.min(distance / rect.width, 1);
			const tx = signedDistance === 0 ? 0 : -Math.sign(signedDistance) * OVERLAP_PX * fraction;
			return {
				scale: 1 - 0.12 * fraction,
				opacity: 1 - 0.35 * fraction,
				z: Math.round((1 - fraction) * 100),
				tx
			};
		});
		depth = nextDepth;
		prevTxByIndex = nextDepth.map((d) => d.tx);
		activeIndex = nearestIndex;
	}

	$effect(() => {
		if (!scrollerEl || cardEls.length === 0) return;

		let ticking = false;
		function onScroll() {
			if (ticking) return;
			ticking = true;
			requestAnimationFrame(() => {
				updateDepth();
				ticking = false;
			});
		}

		updateDepth();
		const el = scrollerEl;
		el.addEventListener('scroll', onScroll, { passive: true });
		window.addEventListener('resize', updateDepth);
		return () => {
			el.removeEventListener('scroll', onScroll);
			window.removeEventListener('resize', updateDepth);
		};
	});
</script>

{#if columns.length > 0}
	<div class="md:hidden">
		<p class="font-label text-xs tracking-widest text-ink/50 uppercase">Slide to browse columns</p>

		<div
			bind:this={scrollerEl}
			class="scrollbar-hide isolate -mx-6 mt-3 flex snap-x snap-mandatory overflow-x-auto px-[11%] pt-2 pb-8"
		>
			{#each columns as column, i (column.category.slug)}
				{@const Icon = getCategoryIcon(column.category.slug)}
				{@const tone = column.category.folderColor ?? getCategoryTabColor(column.category.slug)}
				{@const ink = getCategoryTextColor(tone)}
				{@const d = depth[i] ?? { scale: 1, opacity: 1, z: 0, tx: 0 }}
				<a
					bind:this={cardEls[i]}
					href="/category/{column.category.slug}"
					class="mobile-category-card relative flex w-[78%] shrink-0 snap-center flex-col rounded-2xl p-5"
					style="--tone-raw: {tone}; --ink-on-tone-raw: {ink}; transform: translateX({d.tx}px) scale({d.scale}); opacity: {d.opacity}; z-index: {d.z}; box-shadow: 0 {8 *
						d.scale}px {20 * d.scale}px rgba(20, 18, 12, {0.25 * d.scale});"
				>
					<div class="flex items-start justify-between">
						<Icon class="h-6 w-6" aria-hidden="true" />
						<span class="font-label text-[11px] tracking-widest uppercase opacity-70">
							{column.posts.length}
							{column.posts.length === 1 ? 'post' : 'posts'}
						</span>
					</div>
					<h3 class="mt-4 font-label text-xl tracking-widest uppercase">{column.category.name}</h3>
					<p class="mt-2 text-sm opacity-80">{column.category.description}</p>
					<div class="mt-auto pt-4">
						<div class="h-px w-full opacity-25" style="background: currentColor;"></div>
						<span
							class="mt-4 inline-flex items-center gap-1 rounded-full border px-3 py-1.5 font-label text-xs tracking-widest uppercase"
							style="border-color: currentColor;"
						>
							View Column
							<ChevronRight class="h-3.5 w-3.5" aria-hidden="true" />
						</span>
					</div>
				</a>
			{/each}
		</div>

		{#if activeColumn}
			<div class="mt-2">
				<p class="font-label text-xs tracking-widest text-ink/50 uppercase">
					Latest from this column
				</p>
				<div class="mt-3">
					{#if activeFeaturedPost}
						<FeaturedPostCard post={activeFeaturedPost} color={activeAccentColor} />
					{:else}
						<p class="rounded-lg border border-ink/15 p-6 text-sm text-ink/60">
							No posts in this column yet.
						</p>
					{/if}
				</div>
				{#if activeFeaturedPost}
					<a
						href="/category/{activeColumn.category.slug}"
						class="mt-4 flex items-center justify-center gap-1.5 rounded-full border border-ink/20 py-2.5 font-label text-xs tracking-widest uppercase hover:border-ink/40"
					>
						View all posts in column
						<ChevronRight class="h-3.5 w-3.5" aria-hidden="true" />
					</a>
				{/if}
			</div>
		{/if}
	</div>
{/if}
