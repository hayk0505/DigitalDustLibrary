export function formatDispatchDate(iso: string): string {
	return new Date(iso)
		.toLocaleDateString('en-US', { month: 'short', day: '2-digit', timeZone: 'UTC' })
		.toUpperCase();
}
