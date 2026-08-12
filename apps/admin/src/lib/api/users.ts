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
      //
      // exact: true on this one specifically — without it, invalidating
      // ['users'] prefix-matches ['users', id, 'deletion-impact'] too. That
      // query is still mounted and enabled while the confirm dialog closes
      // (DeleteUserDialog only disables it once `open` flips false, which
      // happens after this onSuccess runs), so the broad invalidation
      // triggers one more fetch for a user that's already gone — a
      // guaranteed 404, plus a spurious error toast landing right on top of
      // the "User deleted" success toast. ['posts']/['media']/['activity']
      // don't have this problem: their sub-keyed variants are real "list
      // still valid, just needs refreshing" queries, not "this exact
      // now-deleted entity" ones.
      queryClient.invalidateQueries({ queryKey: ['users'], exact: true })
      queryClient.invalidateQueries({ queryKey: ['posts'] })
      queryClient.invalidateQueries({ queryKey: ['media'] })
      queryClient.invalidateQueries({ queryKey: ['activity'] })
      toast.success('User deleted')
    },
  })
}
