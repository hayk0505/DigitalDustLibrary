export type Role = 'author' | 'editor' | 'owner'

export interface User {
  id: string
  name: string
  email: string
  role: Role
}

export type PostStatus = 'draft' | 'pending_review' | 'changes_requested' | 'published'

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
  categoryId: string
  categoryName: string
  categoryColor: string
  status: PostStatus
  authorId: string
  authorName: string
  updatedAt: string
  publishedAt: string | null
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

export interface Category {
  id: string
  name: string
  slug: string
  description: string
  color: string
  position: number
  isVisible: boolean
  isDeleted: boolean
  postCount: number
}

export interface ManagedUser {
  id: string
  name: string
  email: string
  role: Role
  isActive: boolean
  createdAt: string
}

export interface UserDeletionImpact {
  postCount: number
  mediaCount: number
  reviewNoteCount: number
  affectedOtherPostCount: number
}

export type ApplicationStatus = 'pending' | 'approved' | 'rejected'

export interface AuthorApplication {
  id: string
  name: string
  email: string
  pitch: string
  status: ApplicationStatus
  submittedAt: string
  reviewedAt: string | null
  devInviteUrl: string | null
}

export interface DirectAddAuthorResponse {
  user: ManagedUser
  devInviteUrl: string | null
}

export interface SiteSettings {
  id: string
  siteTitle: string
  tagline: string
  defaultMetaDescription: string
  linkedInUrl: string
  xUrl: string
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
