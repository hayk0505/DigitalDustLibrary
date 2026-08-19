<script lang="ts">
	import type { Snippet } from 'svelte';
	import { X } from 'lucide-svelte';
	import { positionAnchoredPanel, positionDockedPanel } from '$lib/utils/anchor-panel';

	let {
		class: triggerClass = '',
		children,
		dockLeft
	}: { class?: string; children?: Snippet; dockLeft?: number } = $props();

	type Heading = { id: string; text: string; level: 2 | 3 };

	const PANEL_WIDTH = 300;

	let open = $state(false);
	let headings = $state<Heading[]>([]);
	let triggerEl = $state<HTMLButtonElement | null>(null);
	let panelEl = $state<HTMLDivElement | null>(null);
	let top = $state(0);
	let left = $state(0);

	function slugify(text: string): string {
		return text.trim().toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/^-|-$/g, '') || 'section';
	}

	// The article body's HTML comes from ArticleBody's {@html} render, not
	// from data this component has direct access to — so contents are built
	// by reading the already-rendered DOM rather than re-parsing bodyHtml.
	// IDs are assigned here (not baked into bodyHtml) since no other feature
	// needs them yet; if that changes, generate them once server-side instead
	// and drop this scan.
	function collectHeadings() {
		const article = document.querySelector('.article-body');
		if (!article) return;
		const used = new Set<string>();
		const found: Heading[] = [];
		article.querySelectorAll('h2, h3').forEach((node) => {
			const heading = node as HTMLElement;
			if (!heading.id) {
				const base = slugify(heading.textContent ?? '');
				let id = base;
				let n = 2;
				while (used.has(id)) id = `${base}-${n++}`;
				heading.id = id;
			}
			used.add(heading.id);
			found.push({
				id: heading.id,
				text: heading.textContent ?? '',
				level: heading.tagName === 'H3' ? 3 : 2
			});
		});
		headings = found;
	}

	function reposition() {
		if (!triggerEl || !panelEl) return;
		({ top, left } =
			dockLeft !== undefined
				? positionDockedPanel(triggerEl, dockLeft)
				: positionAnchoredPanel(triggerEl, panelEl, PANEL_WIDTH));
	}

	function openPanel() {
		collectHeadings();
		open = true;
	}

	$effect(() => {
		if (open && triggerEl && panelEl) reposition();
	});

	function close() {
		open = false;
	}

	function jumpTo(id: string) {
		document.getElementById(id)?.scrollIntoView({ behavior: 'smooth', block: 'start' });
		close();
	}

	function onKeydown(e: KeyboardEvent) {
		if (e.key === 'Escape') close();
	}
</script>

<svelte:window onkeydown={open ? onKeydown : undefined} onresize={open ? reposition : undefined} />

<button
	type="button"
	bind:this={triggerEl}
	onclick={openPanel}
	class={triggerClass}
	aria-label="Contents"
	aria-expanded={open}
	aria-haspopup="dialog"
	title="Contents"
>
	{@render children?.()}
</button>

{#if open}
	<button
		type="button"
		class="dd-reader-panel-backdrop"
		onclick={close}
		aria-label="Close contents"
	></button>
	<div
		class="dd-reader-panel"
		bind:this={panelEl}
		style="top: {top}px; left: {left}px; width: {PANEL_WIDTH}px;"
		role="dialog"
		aria-modal="true"
		aria-label="Contents"
	>
		<div class="dd-reader-panel-header">
			<span>Contents</span>
			<button type="button" onclick={close} aria-label="Close contents">
				<X class="h-3.5 w-3.5" aria-hidden="true" />
			</button>
		</div>
		{#if headings.length === 0}
			<p class="dd-reader-panel-empty">No sections in this article.</p>
		{:else}
			<nav class="dd-reader-toc">
				{#each headings as heading (heading.id)}
					<button
						type="button"
						class="dd-reader-toc-item"
						class:is-sub={heading.level === 3}
						onclick={() => jumpTo(heading.id)}
					>
						{heading.text}
					</button>
				{/each}
			</nav>
		{/if}
	</div>
{/if}
