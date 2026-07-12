export function getInitials(name: string): string {
	return name
		.split(' ')
		.filter(Boolean)
		.map((part) => part[0]?.toUpperCase() ?? '')
		.join('')
		.slice(0, 2);
}
