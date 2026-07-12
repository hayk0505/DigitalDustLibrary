<script lang="ts">
	import { getInitials } from '$lib/utils/initials';

	let {
		name,
		colorClass,
		size = 'md'
	}: { name: string; colorClass: string; size?: 'sm' | 'md' } = $props();

	const sizeClasses = size === 'sm' ? 'h-6 w-6 text-[10px]' : 'h-8 w-8 text-xs';
</script>

<span
	class="inline-flex items-center justify-center rounded-full font-label font-medium text-white {colorClass} {sizeClasses}"
>
	{getInitials(name)}
</span>
