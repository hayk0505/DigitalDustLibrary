<script lang="ts">
	import { page } from '$app/state';
	import SiteHeader from '$lib/components/layout/SiteHeader.svelte';
	import CategoryDot from '$lib/components/shared/CategoryDot.svelte';
</script>

{#if page.status === 404}
	<div class="flex min-h-screen flex-col bg-paper">
		<SiteHeader />

		<div
			class="flex items-center justify-between border-b border-ink/10 px-6 py-2.5 font-label text-[11px] tracking-widest text-ink/50 uppercase sm:px-10"
		>
			<span>Error 404 — page not found</span>
			<span class="hidden sm:inline">This URL has rotted away</span>
		</div>

		<main class="flex flex-1 items-center justify-center px-6 py-12 sm:px-10">
			<div class="ddl-fade mx-auto max-w-xl text-center">
				<div class="relative inline-block">
					<div
						class="font-display text-[clamp(88px,18vh,150px)] leading-[0.82] font-bold tracking-tight text-ink"
					>
						4<span class="ddl-drift inline-block text-accent-red">0</span>4
					</div>
					<div
						class="absolute top-2 -right-3.5 origin-top-right rotate-90 font-label text-[11px] tracking-widest text-ink/30 uppercase"
					>
						missing
					</div>
				</div>

				<h1 class="mt-1 font-display text-3xl font-bold tracking-tight">
					This dispatch has turned to dust
				</h1>
				<p class="mx-auto mt-3 max-w-md text-lg leading-relaxed text-ink/60">
					The page you were looking for lapsed, migrated, or was quietly forgotten — exactly the
					kind of decay we write about. The link is gone, but the archive remains.
				</p>

				<div class="mt-6">
					<a
						href="/"
						class="inline-flex items-center gap-2 border border-ink bg-ink px-6 py-3 font-label text-xs tracking-widest text-paper uppercase hover:bg-ink/90"
					>
						← Back to the library
					</a>
				</div>

				<div
					class="mt-5 flex items-center justify-center gap-6 font-label text-[11px] tracking-widest text-ink/50 uppercase"
				>
					<span class="flex items-center gap-1.5">
						<CategoryDot color="var(--color-accent-red)" /> Tech
					</span>
					<span class="flex items-center gap-1.5">
						<CategoryDot color="var(--color-accent-green)" /> Social·Psych
					</span>
					<span class="flex items-center gap-1.5">
						<CategoryDot color="var(--color-accent-blue)" /> Software
					</span>
				</div>
			</div>
		</main>
	</div>
{:else}
	<div
		class="flex min-h-screen flex-col items-center justify-center gap-3 bg-paper px-6 text-center"
	>
		<p class="font-label text-xs tracking-widest text-ink/50 uppercase">Error {page.status}</p>
		<h1 class="font-display text-3xl font-bold">
			{page.error?.message ?? 'Something went missing'}
		</h1>
		<a
			href="/"
			class="font-label text-xs tracking-widest text-accent-red uppercase hover:underline"
		>
			← Back to all columns
		</a>
	</div>
{/if}

<style>
	@keyframes ddl-fade {
		from {
			opacity: 0;
			transform: translateY(14px);
		}
		to {
			opacity: 1;
			transform: translateY(0);
		}
	}
	@keyframes ddl-drift {
		0%,
		100% {
			transform: translateY(0);
		}
		50% {
			transform: translateY(-10px);
		}
	}
	.ddl-fade {
		animation: ddl-fade 0.5s cubic-bezier(0.2, 0.7, 0.2, 1);
	}
	.ddl-drift {
		animation: ddl-drift 4s ease-in-out infinite;
	}
</style>
