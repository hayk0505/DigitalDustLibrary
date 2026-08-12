import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { apiFetch } from './client'
import type { AuthorApplication, DirectAddAuthorResponse } from '@/lib/types'

export function useApplications(options?: { enabled?: boolean }) {
  return useQuery({
    queryKey: ['applications'],
    queryFn: () => apiFetch<AuthorApplication[]>('/applications'),
    enabled: options?.enabled,
  })
}

export function useApproveApplication() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => apiFetch<AuthorApplication>(`/applications/${id}/approve`, { method: 'POST' }),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['applications'] })
      if (data.devInviteUrl) {
        toast.success('Application approved — email not sent, share this link manually', {
          action: { label: 'Copy invite link', onClick: () => navigator.clipboard.writeText(data.devInviteUrl!) },
        })
      } else {
        toast.success('Application approved — invite email sent')
      }
    },
  })
}

export function useRejectApplication() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => apiFetch<AuthorApplication>(`/applications/${id}/reject`, { method: 'POST' }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['applications'] })
      toast.success('Application rejected')
    },
  })
}

export function useDirectAddAuthor() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (author: { name: string; email: string }) =>
      apiFetch<DirectAddAuthorResponse>('/applications/direct', { method: 'POST', body: JSON.stringify(author) }),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['users'] })
      if (data.devInviteUrl) {
        toast.success('Author added — email not sent, share this link manually', {
          action: { label: 'Copy invite link', onClick: () => navigator.clipboard.writeText(data.devInviteUrl!) },
        })
      } else {
        toast.success('Author added — invite email sent')
      }
    },
  })
}
