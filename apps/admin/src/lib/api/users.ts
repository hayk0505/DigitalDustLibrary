import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { apiFetch } from './client'
import type { ManagedUser, UserDeletionImpact } from '@/lib/types'

export function useUsers(options?: { enabled?: boolean }) {
  return useQuery({
    queryKey: ['users'],
    queryFn: () => apiFetch<ManagedUser[]>('/users'),
    enabled: options?.enabled,
  })
}

export function useUpdateUser() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, ...patch }: { id: string } & Partial<Pick<ManagedUser, 'role' | 'isActive'>>) =>
      apiFetch<ManagedUser>(`/users/${id}`, { method: 'PATCH', body: JSON.stringify(patch) }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] })
      toast.success('User updated')
    },
  })
}

export function useUserDeletionImpact(id: string, options?: { enabled?: boolean }) {
  return useQuery({
    queryKey: ['users', id, 'deletion-impact'],
    queryFn: () => apiFetch<UserDeletionImpact>(`/users/${id}/deletion-impact`),
    enabled: options?.enabled,
  })
}

export function useDeleteUser() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => apiFetch<void>(`/users/${id}`, { method: 'DELETE' }),
    onSuccess: () => {
      // Deleting a user cascades to their posts, media, and writes an
      // activity-log entry, so all four caches need to be invalidated,
      // not just ['users'].
      queryClient.invalidateQueries({ queryKey: ['users'] })
      queryClient.invalidateQueries({ queryKey: ['posts'] })
      queryClient.invalidateQueries({ queryKey: ['media'] })
      queryClient.invalidateQueries({ queryKey: ['activity'] })
      toast.success('User deleted')
    },
  })
}
