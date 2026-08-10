import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { apiFetch } from './client'
import type { AuthorApplication } from '@/lib/types'

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
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['applications'] })
      toast.success('Application approved — invite email sent')
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
