<script lang="ts">
	// html is our own mock data today, never user input — but once a real API/CMS
	// backend supplies author-authored content here, this needs sanitization
	// (e.g. DOMPurify) before @html renders it. Do not remove this note when
	// wiring up the real data layer.
	let { html }: { html: string } = $props();
</script>

<div class="article-body prose prose-neutral mt-8 max-w-none">
	<!-- eslint-disable-next-line svelte/no-at-html-tags -- html is our own mock data, never user input; see comment in <script> -->
	{@html html}
</div>

<style>
	/* Author/scraped content can't be trusted to always contain a break
	   opportunity (e.g. a long URL, or — as found on a real test article —
	   a run of table-cell text that lost its spaces on the way into this
	   field). Without this, one such run forces the whole page wider than
	   the viewport instead of wrapping, which is easy to miss on the
	   normal article column (768px, rarely narrow enough to hit) but shows
	   up reliably on Reader Mode's narrower mobile column. */
	.article-body {
		overflow-wrap: break-word;
	}

	.article-body :global(p:first-of-type::first-letter) {
		float: left;
		margin-right: 0.5rem;
		font-family: var(--font-display);
		font-size: 4.5rem;
		font-weight: 700;
		line-height: 0.85;
		color: var(--color-accent-blue);
	}
</style>
