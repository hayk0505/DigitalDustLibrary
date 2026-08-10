<script lang="ts">
	import { getAvatarColor } from '$lib/utils/avatar-color';
	import Avatar from './Avatar.svelte';

	let { author }: { author: { name: string; handle: string } } = $props();
</script>

<div class="flex items-center gap-2">
	<Avatar name={author.name} colorClass={getAvatarColor(author.handle)} />
	<p class="text-sm font-semibold leading-tight">{author.name}</p>
</div>
