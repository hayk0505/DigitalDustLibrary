import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { apiFetch } from './client'
import type { MediaAsset, MediaTag } from '@/lib/types'

export function useMedia(search?: string) {
  const query = search ? `?search=${encodeURIComponent(search)}` : ''
  return useQuery({
    queryKey: ['media', search ?? ''],
    queryFn: () => apiFetch<MediaAsset[]>(`/media${query}`),
  })
}

export function useUploadMedia() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (asset: { filename: string; dataUrl: string; tag: MediaTag; width: number; height: number }) =>
      apiFetch<MediaAsset>('/media', { method: 'POST', body: JSON.stringify(asset) }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['media'] })
      toast.success('Asset uploaded')
    },
  })
}
