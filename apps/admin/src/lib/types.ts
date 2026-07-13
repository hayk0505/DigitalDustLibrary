export type Role = 'author' | 'editor' | 'owner'

export interface User {
  id: string
  name: string
  email: string
  role: Role
}

export type PostStatus = 'draft' | 'pending_review' | 'changes_requested' | 'published'

export type Pillar = 'tech' | 'social_psych' | 'software_dev'

export interface ReviewNote {
  id: string
  comment: string
  reviewerName: string
  createdAt: string
}

export interface Post {
  id: string
  title: string
  bodyHtml: string
  excerpt: string
  seoTitle: string
  metaDescription: string
  featuredImageId: string | null
  pillar: Pillar
  status: PostStatus
  authorId: string
  updatedAt: string
  latestReviewNote: ReviewNote | null
}

export type MediaTag = 'featured' | 'inline' | 'og_image' | 'avatar'

export interface MediaAsset {
  id: string
  filename: string
  tag: MediaTag
  width: number
  height: number
  url: string
}

export interface AuthResponse {
  accessToken: string
  user: User
}

export interface ActivityEvent {
  id: string
  actorName: string
  action: string
  createdAt: string
}
