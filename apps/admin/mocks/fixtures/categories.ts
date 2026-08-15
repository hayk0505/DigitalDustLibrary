import type { Category } from '@/lib/types'

export const categories: Category[] = [
  { id: 'cat-tech', name: 'Tech', slug: 'tech', description: 'Where the industry\'s tools and infrastructure get taken apart.', color: '#C9553D', folderColor: '#667384', position: 1, isVisible: true, isDeleted: false, postCount: 12 },
  { id: 'cat-social', name: 'Social · Psych', slug: 'social-psych', description: 'Field notes on attention, identity, and the internet\'s effect on how people think.', color: '#3F8F6A', folderColor: null, position: 2, isVisible: true, isDeleted: false, postCount: 8 },
  { id: 'cat-dev', name: 'Software Dev', slug: 'software-dev', description: 'The craft side of building software.', color: '#4A6FBF', folderColor: null, position: 3, isVisible: true, isDeleted: false, postCount: 15 },
  { id: 'cat-interviews', name: 'Interviews', slug: 'interviews', description: 'Conversations with people doing interesting work.', color: '#A27B5B', folderColor: null, position: 4, isVisible: false, isDeleted: false, postCount: 0 },
  { id: 'cat-old-series', name: 'Old Series', slug: 'old-series', description: 'A retired series, kept for archive purposes.', color: '#8A8A8A', folderColor: null, position: 5, isVisible: true, isDeleted: true, postCount: 0 },
]
