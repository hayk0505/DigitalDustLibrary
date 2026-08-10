import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { apiFetch } from './client'
import type { Post } from '@/lib/types'

export function usePosts(filter?: { mine?: boolean }, options?: { enabled?: boolean }) {
  const query = filter?.mine ? '?mine=true' : ''
  return useQuery({
    queryKey: ['posts', filter ?? {}],
    queryFn: () => apiFetch<Post[]>(`/posts${query}`),
    enabled: options?.enabled,
  })
}

export function useCreatePost() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (post: Partial<Post>) => apiFetch<Post>('/posts', { method: 'POST', body: JSON.stringify(post) }),
    onSuccess: (post) => {
      queryClient.invalidateQueries({ queryKey: ['posts'] })
      toast.success(post.status === 'pending_review' ? 'Submitted for review' : 'Draft saved')
    },
  })
}

export function useUpdatePost() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, ...patch }: { id: string } & Partial<Post>) =>
      apiFetch<Post>(`/posts/${id}`, { method: 'PATCH', body: JSON.stringify(patch) }),
    onSuccess: (post) => {
      queryClient.invalidateQueries({ queryKey: ['posts'] })
      toast.success(post.status === 'pending_review' ? 'Submitted for review' : 'Draft saved')
    },
  })
}

export function useApprovePost() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => apiFetch<Post>(`/posts/${id}/approve`, { method: 'POST' }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['posts'] })
      toast.success('Post published')
    },
  })
}

export function useRequestChanges() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, comment }: { id: string; comment: string }) =>
      apiFetch<Post>(`/posts/${id}/request-changes`, { method: 'POST', body: JSON.stringify({ comment }) }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['posts'] })
      toast.success('Changes requested')
    },
  })
}
