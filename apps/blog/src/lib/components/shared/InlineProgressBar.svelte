<script lang="ts">
	let { progress }: { progress: number } = $props();
</script>

<div class="h-[3px] w-full bg-ink/10">
	<div class="h-full bg-accent-red" style="width: {progress}%"></div>
</div>
