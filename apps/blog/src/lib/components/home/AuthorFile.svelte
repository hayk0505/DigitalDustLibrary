<script lang="ts">
	import { page } from '$app/state';
	import { slide } from 'svelte/transition';
	import type { Author, Post } from '$lib/data';
	import { getAvatarColor } from '$lib/utils/avatar-color';
	import { getInitials } from '$lib/utils/initials';
	import Avatar from '../shared/Avatar.svelte';

	const author = $derived((page.data.author as Author | null | undefined) ?? null);
	const posts = $derived((page.data.posts as Post[] | undefined) ?? []);

	const authorPosts = $derived(author ? posts.filter((p) => p.authorHandle === author.handle) : []);
	const categoryTags = $derived([...new Set(authorPosts.map((p) => p.categoryName))].join(' · '));
	const memberSinceYear = $derived(author ? new Date(author.createdAt).getFullYear() : null);

	// Same rolling-hash shape as category-visuals.ts's getCategoryTabColor —
	// a stable per-author "file number" for the archival-card flourish.
	// Decorative only, not a real record ID.
	function hashString(seed: string): number {
		let hash = 0;
		for (let i = 0; i < seed.length; i++) hash = (hash * 31 + seed.charCodeAt(i)) | 0;
		return Math.abs(hash);
	}
	const fileNumber = $derived(author ? String((hashString(author.handle) % 999) + 1).padStart(3, '0') : '');
</script>

{#if author}
	<div class="author-file" transition:slide={{ axis: 'x', duration: 280 }}>
		<div class="author-file-tag">
			<span>Author File</span>
			<span class="author-file-id">{getInitials(author.name)}-{fileNumber}</span>
		</div>

		<Avatar name={author.name} colorClass={getAvatarColor(author.handle)} size="lg" />

		<h2 class="author-file-name">{author.name}</h2>

		{#if author.bio}
			<p class="author-file-bio">{author.bio}</p>
		{/if}

		<div class="author-file-sep"></div>

		<div>
			<span class="author-file-stat-num">{authorPosts.length}</span>
			<span class="author-file-stat-label">Articles</span>
		</div>

		<div class="author-file-sep"></div>

		{#if categoryTags}
			<p class="author-file-meta">{categoryTags}</p>
		{/if}
		{#if memberSinceYear}
			<p class="author-file-meta">Member since {memberSinceYear}</p>
		{/if}

		<a href="/authors/{author.handle}" class="author-file-link">View author →</a>
	</div>
{/if}
