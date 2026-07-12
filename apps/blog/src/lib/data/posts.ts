import type { Post } from './types';

export const posts: Post[] = [
	{
		slug: 'reading-the-rings-of-a-data-center',
		title: 'Reading the Rings of a Data Center',
		excerpt:
			'Racks age in visible layers. You can date a facility by its cabling the way you date a tree by its rings.',
		pillarSlug: 'tech',
		authorHandle: 'priya-anand',
		publishedAt: '2026-06-15',
		readingMinutes: 7,
		dispatchNumber: 1,
		featured: false,
		body: `<p>Racks age in visible layers. You can date a facility by its cabling the way you date a tree by its rings — the color of the patch cables, the generation of switch gear, the dust pattern behind a cooling unit that's been running since a decommissioned product launch.</p>
<p>Walk any old data hall long enough and you can point at a row and say: this is when we still believed in that acquisition. The hardware doesn't lie about what mattered when it was installed, even after everyone who installed it has moved on.</p>`
	},
	{
		slug: 'cold-storage-and-the-myth-of-permanence',
		title: 'Cold Storage and the Myth of Permanence',
		excerpt:
			'Tape survives longer than the company that wrote to it. A tour of the places we send data to be forgotten slowly.',
		pillarSlug: 'tech',
		authorHandle: 'maren-osei',
		publishedAt: '2026-06-24',
		readingMinutes: 9,
		dispatchNumber: 2,
		featured: false,
		body: `<p>Tape survives longer than the company that wrote to it. Walk into any long-term archival facility and you'll find drives spinning down decades after the businesses that filled them went quiet.</p>
<p>There's something almost comic about it: the format outlives the reason it was chosen. LTO tape was picked for cost, not sentiment, and yet it's the closest thing the industry has to permanence — not because it's built to last forever, but because nobody's finished migrating off it yet.</p>
<p>Permanence, it turns out, is mostly a migration schedule nobody's gotten around to running.</p>`
	},
	{
		slug: 'what-the-internet-forgets-on-purpose',
		title: 'What the Internet Forgets on Purpose',
		excerpt:
			'Deletion is rarely an accident. Someone, somewhere, decided this was not worth the storage bill.',
		pillarSlug: 'tech',
		authorHandle: 'theo-vance',
		publishedAt: '2026-07-02',
		readingMinutes: 8,
		dispatchNumber: 3,
		featured: false,
		body: `<p>Deletion is rarely an accident. Someone, somewhere, decided this was not worth the storage bill, and a form that used to exist stopped existing, quietly, without a farewell tour.</p>
<p>We like to talk about the internet forgetting things as if forgetting were a failure of the system. Mostly it's the opposite: a decision, made by someone with a budget, about what counted as worth keeping. The blob storage bill doesn't care about your nostalgia.</p>
<p>What's left after a purge like that tells you more about a company's priorities than its mission statement ever could. Look at what survived the last cost-cutting pass, and you've found what they actually valued.</p>`
	},
	{
		slug: 'half-life-of-a-hyperlink',
		title: 'The Half-Life of a Hyperlink',
		excerpt:
			'Every link you post is quietly counting down. A field guide to link rot, and why the average URL outlives a housefly by only a few years.',
		pillarSlug: 'tech',
		authorHandle: 'maren-osei',
		publishedAt: '2026-07-04',
		readingMinutes: 11,
		dispatchNumber: 4,
		featured: true,
		body: `<p>Every link you post is quietly counting down from the moment you hit publish. Domains lapse, companies fold, content management systems get replaced by other content management systems, and the URL that felt permanent turns out to have been rented, not owned.</p>
<p>Researchers who have tried to measure this call it link rot, and the numbers are worse than most people expect: a meaningful fraction of links in any given web page are dead within a few years, and the average shared URL outlives a housefly by only a handful of summers. The web remembers less than we think it does, and it forgets faster than we're built to notice.</p>
<p>The fix isn't heroic — it's boring, which is why almost nobody does it. Archive what you link to. Prefer permalinks with stable identifiers over query strings that break the moment a site redesigns. Treat every outbound link in something you've written as a small promise you probably won't be able to keep.</p>`
	},
	{
		slug: 'digital-grief-and-the-ghosts-in-our-inboxes',
		title: 'Digital Grief and the Ghosts in Our Inboxes',
		excerpt:
			'The dead keep sending calendar invites. On mourning people whose accounts outlive them.',
		pillarSlug: 'social-psych',
		authorHandle: 'lena-hart',
		publishedAt: '2026-06-11',
		readingMinutes: 10,
		dispatchNumber: 5,
		featured: false,
		body: `<p>The dead keep sending calendar invites. Birthday reminders fire on schedule, autoplay suggests a video from an account that hasn't posted in three years, and a subscription renews for a service its owner will never log into again.</p>
<p>We built systems that assume everyone using them is still alive, and now we're stuck maintaining etiquette for a situation none of the interfaces were designed to hold.</p>`
	},
	{
		slug: 'why-we-hoard-screenshots-we-never-reopen',
		title: 'Why We Hoard Screenshots We Never Reopen',
		excerpt:
			'Ten thousand images in the camera roll, opened once. On saving as a substitute for remembering.',
		pillarSlug: 'social-psych',
		authorHandle: 'ada-reyes',
		publishedAt: '2026-06-21',
		readingMinutes: 6,
		dispatchNumber: 6,
		featured: false,
		body: `<p>Ten thousand images in the camera roll, opened once. On saving as a substitute for remembering — the screenshot as a promise to your future self that you'll come back to this, a promise you both know you won't keep.</p>
<p>The saving itself is the relief. Once it's captured, the brain quietly reclassifies the moment as handled, filed, no longer its job to hold onto. The archive grows; the memory doesn't.</p>`
	},
	{
		slug: 'the-loneliness-of-the-long-scrolling-user',
		title: 'The Loneliness of the Long-Scrolling User',
		excerpt: 'Infinite feeds promised company and delivered a very crowded kind of solitude.',
		pillarSlug: 'social-psych',
		authorHandle: 'jonah-pike',
		publishedAt: '2026-06-30',
		readingMinutes: 9,
		dispatchNumber: 8,
		featured: false,
		body: `<p>Infinite feeds promised company and delivered a very crowded kind of solitude — a room full of people, none of whom know you're there.</p>
<p>The design is not an accident. Engagement is easier to sell than connection, and a feed that never ends is a feed that never has to justify why you're still watching.</p>`
	},
	{
		slug: 'nostalgia-as-a-compression-algorithm',
		title: 'Nostalgia as a Compression Algorithm',
		excerpt:
			'Memory does not store the past; it stores a lossy summary and reconstructs the rest. What the internet does to us, we already did to ourselves.',
		pillarSlug: 'social-psych',
		authorHandle: 'ada-reyes',
		publishedAt: '2026-07-03',
		readingMinutes: 12,
		dispatchNumber: 7,
		featured: true,
		body: `<p>Memory does not store the past; it stores a lossy summary and reconstructs the rest on demand, filling gaps with whatever fits the shape of the story you already believe about yourself.</p>
<p>This isn't a flaw so much as an engineering tradeoff. A brain that stored every frame at full fidelity would run out of room by adolescence. So it keeps the compressed version — the highlight reel, the emotional average — and discards the raw footage.</p>
<p>What the internet does to us, with its algorithmic feeds resurfacing old photos on cue, we already did to ourselves first. It just automated the part we used to do quietly, alone, at 2 a.m.</p>`
	},
	{
		slug: 'comments-are-messages-to-a-stranger',
		title: 'Comments Are Messages to a Stranger',
		excerpt:
			'You write them for a future maintainer you will never meet. Usually that maintainer is you, and you are furious.',
		pillarSlug: 'software-dev',
		authorHandle: 'iris-wong',
		publishedAt: '2026-06-09',
		readingMinutes: 6,
		dispatchNumber: 9,
		featured: false,
		body: `<p>You write them for a future maintainer you will never meet. Usually that maintainer is you, and you are furious — furious that past-you didn't explain the workaround, furious that the comment you're about to write will be read by someone equally unprepared.</p>
<p>The best comments aren't documentation. They're apologies, written in advance, for a decision that made sense under a deadline and won't make sense to anyone reading it cold.</p>`
	},
	{
		slug: 'deprecation-is-a-kind-of-mourning',
		title: 'Deprecation Is a Kind of Mourning',
		excerpt:
			'Sunsetting an API means telling everyone who depended on it that their world is ending — politely, in a changelog.',
		pillarSlug: 'software-dev',
		authorHandle: 'iris-wong',
		publishedAt: '2026-06-19',
		readingMinutes: 7,
		dispatchNumber: 10,
		featured: false,
		body: `<p>Sunsetting an API means telling everyone who depended on it that their world is ending — politely, in a changelog, with a migration guide nobody reads until the day it matters.</p>
<p>There's a particular kind of guilt in writing a deprecation notice. You're not just removing a function; you're informing strangers, some of whom built entire businesses on your promise that this would keep working, that the promise had a shelf life you never advertised.</p>`
	},
	{
		slug: 'the-archaeology-of-a-node-modules-folder',
		title: 'The Archaeology of a node_modules Folder',
		excerpt:
			'Dig deep enough and you find abandoned packages, dead maintainers, and forks of forks of forks.',
		pillarSlug: 'software-dev',
		authorHandle: 'sam-okafor',
		publishedAt: '2026-06-28',
		readingMinutes: 8,
		dispatchNumber: 11,
		featured: false,
		body: `<p>Dig deep enough and you find abandoned packages, dead maintainers, and forks of forks of forks — a dependency tree is a family tree, and like most family trees, nobody agrees on who's actually still speaking to whom.</p>
<p>Every node_modules folder is a small museum of decisions nobody remembers making: a polyfill for a browser nobody supports anymore, a utility library pulled in for one function that could've been six lines.</p>`
	},
	{
		slug: 'legacy-code-is-a-love-letter',
		title: 'Legacy Code Is a Love Letter',
		excerpt:
			'The ugliest function in the codebase is usually the one that kept the company alive. A defense of the code nobody wants to touch.',
		pillarSlug: 'software-dev',
		authorHandle: 'sam-okafor',
		publishedAt: '2026-07-07',
		readingMinutes: 10,
		dispatchNumber: 12,
		featured: true,
		body: `<p>For a long time we told ourselves that the web was permanent — that once a thing was posted it was posted forever, indexed and immortal. The truth is closer to the opposite. What we publish begins decaying the moment it lands, and most of it is gone within a decade, quietly, without ceremony or obituary.</p>
<p>The mechanisms of this decay are dull. A company folds. A subdomain lapses. A migration goes half-finished and the old URLs stop resolving. No villain, no fire — just entropy doing the unglamorous work it always does, one dead link at a time.</p>
<p>What interests me is not the loss itself but what the loss reveals about how we valued the thing in the first place. We keep what we are paid to keep, and we lose almost everything else. The archive is not a record of what mattered; it is a record of what was profitable to remember.</p>
<p>There is a version of this story that is purely mournful, and I have told it that way before. But there is another reading, and lately I prefer it: forgetting is not a bug in the system — it's a feature of how anything alive stays legible. A codebase that remembered every dead branch, every abandoned experiment, every function nobody had the heart to delete, would not be a codebase. It would be a museum with the lights left on in every room at once.</p>
<p>That is what legacy code actually is, underneath the jokes about spaghetti and the groaning in code review. It is a record of every deadline the company survived by choosing done over clean. The ugliest function in the repository is rarely a mistake — it is a scar, and scars are proof that something kept living long enough to get one.</p>`
	}
];
