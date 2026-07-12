export function formatDispatchDate(iso: string): string {
	// timeZone: 'UTC' avoids an off-by-one day for users behind UTC, since publish
	// dates are stored as bare YYYY-MM-DD with no time component.
	return new Date(iso)
		.toLocaleDateString('en-US', { month: 'short', day: '2-digit', timeZone: 'UTC' })
		.toUpperCase();
}
