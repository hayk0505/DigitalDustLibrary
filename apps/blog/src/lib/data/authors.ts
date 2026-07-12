import type { Author } from './types';

export const authors: Author[] = [
	{ handle: 'theo-vance', name: 'Theo Vance', role: 'Tech Desk', avatarColor: 'bg-amber-500' },
	{ handle: 'maren-osei', name: 'Maren Osei', role: 'Tech Desk', avatarColor: 'bg-rose-500' },
	{ handle: 'priya-anand', name: 'Priya Anand', role: 'Tech Desk', avatarColor: 'bg-emerald-500' },
	{ handle: 'ada-reyes', name: 'Ada Reyes', role: 'Culture Desk', avatarColor: 'bg-violet-500' },
	{ handle: 'jonah-pike', name: 'Jonah Pike', role: 'Culture Desk', avatarColor: 'bg-sky-500' },
	{ handle: 'lena-hart', name: 'Lena Hart', role: 'Culture Desk', avatarColor: 'bg-fuchsia-500' },
	{
		handle: 'sam-okafor',
		name: 'Sam Okafor',
		role: 'Engineering Desk',
		avatarColor: 'bg-blue-600'
	},
	{ handle: 'iris-wong', name: 'Iris Wong', role: 'Engineering Desk', avatarColor: 'bg-teal-500' }
];

export function getAuthorByHandle(handle: string): Author | undefined {
	return authors.find((author) => author.handle === handle);
}
