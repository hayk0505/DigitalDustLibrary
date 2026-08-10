import type { Category } from '@/lib/types'

export const categories: Category[] = [
  { id: 'cat-tech', name: 'Tech', slug: 'tech', isPillar: true, isVisible: true, isDeleted: false, postCount: 12 },
  { id: 'cat-social', name: 'Social & Psychological', slug: 'social-psychological', isPillar: true, isVisible: true, isDeleted: false, postCount: 8 },
  { id: 'cat-dev', name: 'Software Development', slug: 'software-development', isPillar: true, isVisible: true, isDeleted: false, postCount: 15 },
  { id: 'cat-interviews', name: 'Interviews', slug: 'interviews', isPillar: false, isVisible: false, isDeleted: false, postCount: 0 },
  { id: 'cat-old-series', name: 'Old Series', slug: 'old-series', isPillar: false, isVisible: true, isDeleted: true, postCount: 0 },
]
