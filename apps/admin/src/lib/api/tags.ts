import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { apiFetch } from './client'
import type { Tag } from '@/lib/types'

export function useTags() {
  return useQuery({
    queryKey: ['tags'],
    queryFn: () => apiFetch<Tag[]>('/tags'),
  })
}

export function useCreateTag() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (tag: { name: string }) => apiFetch<Tag>('/tags', { method: 'POST', body: JSON.stringify(tag) }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tags'] })
      toast.success('Tag created')
    },
  })
}

export function useUpdateTag() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, ...patch }: { id: string } & Partial<Pick<Tag, 'name' | 'slug'>>) =>
      apiFetch<Tag>(`/tags/${id}`, { method: 'PATCH', body: JSON.stringify(patch) }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tags'] })
      toast.success('Tag updated')
    },
  })
}

export function useMergeTag() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, targetTagId }: { id: string; targetTagId: string }) =>
      apiFetch<Tag>(`/tags/${id}/merge`, { method: 'POST', body: JSON.stringify({ targetTagId }) }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tags'] })
      queryClient.invalidateQueries({ queryKey: ['posts'] })
      toast.success('Tags merged')
    },
  })
}

export function useDeleteTag() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => apiFetch<void>(`/tags/${id}`, { method: 'DELETE' }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tags'] })
      toast.success('Tag deleted')
    },
  })
}
