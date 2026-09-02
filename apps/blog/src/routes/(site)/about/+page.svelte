<script lang="ts">
	import { page } from '$app/state';
	import SeoHead from '$lib/components/shared/SeoHead.svelte';

	const settings = $derived(page.data.settings);
	const categories = $derived(page.data.categories ?? []);
	const categoryList = $derived(categories.map((c) => c.name).join(', '));
</script>

<SeoHead
	title="About"
	description="Why Digital Dust Library exists, and what you'll find here."
	url={page.url.href}
/>

<div class="max-w-2xl">
	<h1 class="font-display text-3xl font-bold">About</h1>

	<p class="mt-6 text-ink/70">
		Digital dust is what's left over — the fragments, the half-finished thoughts, the stuff that
		falls out of a working session or a late-night rabbit hole and usually just gets lost. This is
		where I collect it instead of letting it disappear: long-form pieces, written properly instead
		of as skimmable listicles, on whatever's actually worth thinking through.
	</p>

	<h2 class="mt-10 font-display text-xl font-bold">Why this exists</h2>
	<p class="mt-4 text-ink/70">
		Writing is what actually makes me think something through — an idea doesn't really count as
		understood until I've tried to put it into words and it holds up. For a long time that just
		meant notes that went nowhere, half-finished thoughts left in a folder until I forgot why they
		mattered. Digital Dust Library is where that stops. It's a place to finish those thoughts
		properly and put them somewhere real — about the things that keep bothering me, or interest me,
		or won't leave me alone until I've worked them all the way through. Chances are, if something's
		been nagging at me long enough to write about, it's not just a me problem.
	</p>

	<h2 class="mt-10 font-display text-xl font-bold">What you'll find here</h2>
	<p class="mt-4 text-ink/70">
		{#if categoryList}
			Posts are organized loosely by category — right now that's {categoryList}, with more added
			as new subjects earn their own shelf.
		{:else}
			Posts are organized loosely by category, with new ones added as new subjects earn their own
			shelf.
		{/if}
		They also come in two shapes — quick <strong>Minis</strong> for a thought that doesn't need three
		thousand words, and full <strong>Shelf</strong> pieces for the ones that do.
	</p>

	<h2 class="mt-10 font-display text-xl font-bold">Get involved</h2>
	<p class="mt-4 text-ink/70">
		Right now it's just me writing here, but that's not the plan forever — down the line, other
		writers will join Digital Dust Library, each with their own byline and bio. If that's something
		you're interested in, <a href="/become-an-author" class="underline hover:text-accent-red"
			>applications are open</a
		>.
	</p>

	{#if settings?.linkedInUrl || settings?.xUrl}
		<h2 class="mt-10 font-display text-xl font-bold">Connect with me</h2>
		<p class="mt-4 text-ink/70">
			{#if settings.linkedInUrl}
				<a
					href={settings.linkedInUrl}
					target="_blank"
					rel="noopener noreferrer"
					class="underline hover:text-accent-red">LinkedIn</a
				>
			{/if}
			{#if settings.linkedInUrl && settings.xUrl}
				<span class="mx-2">·</span>
			{/if}
			{#if settings.xUrl}
				<a
					href={settings.xUrl}
					target="_blank"
					rel="noopener noreferrer"
					class="underline hover:text-accent-red">X</a
				>
			{/if}
		</p>
	{/if}

	<h2 class="mt-10 font-display text-xl font-bold">Documents</h2>
	<p class="mt-4 text-ink/70">
		<a href="/terms" class="underline hover:text-accent-red">Terms of Service</a>
		<span class="mx-2">·</span>
		<a href="/privacy" class="underline hover:text-accent-red">Privacy Policy</a>
	</p>
</div>
