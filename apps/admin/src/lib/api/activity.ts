import { useQuery } from '@tanstack/react-query'
import { apiFetch } from './client'
import type { ActivityEvent } from '@/lib/types'

export function useActivity(options?: { mine?: boolean; enabled?: boolean }) {
  const query = options?.mine ? '?mine=true' : ''
  return useQuery({
    queryKey: ['activity', { mine: options?.mine ?? false }],
    queryFn: () => apiFetch<ActivityEvent[]>(`/activity${query}`),
    enabled: options?.enabled,
  })
}
