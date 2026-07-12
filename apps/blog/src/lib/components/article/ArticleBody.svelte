<script lang="ts">
	// html always comes from our own mock data, never user input, so @html is safe here.
	let { html }: { html: string } = $props();
</script>

<div class="article-body prose prose-neutral mt-8 max-w-none">
	<!-- eslint-disable-next-line svelte/no-at-html-tags -- html is our own mock data, never user input; see comment in <script> -->
	{@html html}
</div>

<style>
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
