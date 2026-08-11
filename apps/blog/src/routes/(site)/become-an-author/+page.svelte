<script lang="ts">
	import { page } from '$app/state';
	import SeoHead from '$lib/components/shared/SeoHead.svelte';
	import { submitAuthorApplication, ApplicationSubmitError } from '$lib/api';

	let name = $state('');
	let email = $state('');
	let pitch = $state('');
	let status = $state<'idle' | 'submitting' | 'success' | 'error'>('idle');
	let errorMessage = $state('');

	async function handleSubmit(event: SubmitEvent) {
		event.preventDefault();
		if (status === 'submitting') return;

		status = 'submitting';
		try {
			await submitAuthorApplication(fetch, { name, email, pitch });
			status = 'success';
		} catch (error) {
			errorMessage =
				error instanceof ApplicationSubmitError
					? error.message
					: 'Something went wrong. Please try again.';
			status = 'error';
		}
	}
</script>

<SeoHead
	title="Become an Author — Digital Dust Library"
	description="Digital Dust Library is opening up to outside contributors — find out how to apply."
	url={page.url.href}
/>

<div class="max-w-xl">
	<h1 class="font-display text-3xl font-bold">Become an author</h1>

	{#if status === 'success'}
		<p class="mt-4 text-ink/70">
			Thanks for reaching out. We'll review your pitch and be in touch.
		</p>
	{:else}
		<p class="mt-4 text-ink/70">
			Digital Dust Library is opening up to outside contributors. Tell us who you are and what you'd
			like to write about.
		</p>

		<form onsubmit={handleSubmit} class="mt-8 space-y-4">
			<div>
				<label for="name" class="font-label text-xs tracking-widest text-ink/50 uppercase">
					Name
				</label>
				<input
					id="name"
					type="text"
					bind:value={name}
					required
					class="mt-1 w-full rounded-none border border-ink/20 bg-transparent px-3 py-2 focus:border-accent-red focus:outline-none"
				/>
			</div>

			<div>
				<label for="email" class="font-label text-xs tracking-widest text-ink/50 uppercase">
					Email
				</label>
				<input
					id="email"
					type="email"
					bind:value={email}
					required
					class="mt-1 w-full rounded-none border border-ink/20 bg-transparent px-3 py-2 focus:border-accent-red focus:outline-none"
				/>
			</div>

			<div>
				<label for="pitch" class="font-label text-xs tracking-widest text-ink/50 uppercase">
					What do you want to write about?
				</label>
				<textarea
					id="pitch"
					bind:value={pitch}
					required
					minlength="30"
					rows="5"
					class="mt-1 w-full rounded-none border border-ink/20 bg-transparent px-3 py-2 focus:border-accent-red focus:outline-none"
				></textarea>
			</div>

			{#if status === 'error'}
				<p class="text-sm text-accent-red">{errorMessage}</p>
			{/if}

			<button
				type="submit"
				disabled={status === 'submitting'}
				class="rounded-none border border-ink px-4 py-2 font-label text-xs tracking-widest uppercase hover:bg-ink hover:text-paper disabled:opacity-50"
			>
				{status === 'submitting' ? 'Sending…' : 'Submit'}
			</button>
		</form>
	{/if}
</div>
