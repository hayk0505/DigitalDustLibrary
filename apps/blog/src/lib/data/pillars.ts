import type { Pillar, PillarAccent } from './types';

export const pillars: Pillar[] = [
	{ slug: 'tech', label: 'Tech', index: 1, accent: 'red' },
	{ slug: 'social-psych', label: 'Social · Psych', index: 2, accent: 'green' },
	{ slug: 'software-dev', label: 'Software Dev', index: 3, accent: 'blue' }
];

type AccentClasses = {
	dot: string;
	text: string;
	border: string;
	bg: string;
	groupHoverText: string;
};


export const pillarAccentClasses: Record<PillarAccent, AccentClasses> = {
	red: {
		dot: 'bg-accent-red',
		text: 'text-accent-red',
		border: 'border-accent-red',
		bg: 'bg-accent-red',
		groupHoverText: 'group-hover:text-accent-red'
	},
	green: {
		dot: 'bg-accent-green',
		text: 'text-accent-green',
		border: 'border-accent-green',
		bg: 'bg-accent-green',
		groupHoverText: 'group-hover:text-accent-green'
	},
	blue: {
		dot: 'bg-accent-blue',
		text: 'text-accent-blue',
		border: 'border-accent-blue',
		bg: 'bg-accent-blue',
		groupHoverText: 'group-hover:text-accent-blue'
	}
};

export function getPillarBySlug(slug: string): Pillar | undefined {
	return pillars.find((pillar) => pillar.slug === slug);
}
