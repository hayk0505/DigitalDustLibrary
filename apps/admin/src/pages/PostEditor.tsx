import { useEffect, useState } from 'react'
import { useNavigate } from '@tanstack/react-router'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { usePosts, useCreatePost, useUpdatePost } from '@/lib/api/posts'
import { useMedia } from '@/lib/api/media'
import { useAuth } from '@/hooks/useAuth'
import { TipTapEditor } from '@/components/shared/TipTapEditor'
import { MediaLibrary } from './MediaLibrary'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import { Button } from '@/components/ui/button'
import { Dialog, DialogContent, DialogTrigger, DialogTitle } from '@/components/ui/dialog'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import type { Pillar } from '@/lib/types'

const postSchema = z.object({
  title: z.string().min(1, 'Title is required'),
  pillar: z.enum(['tech', 'social_psych', 'software_dev']),
  excerpt: z.string(),
  seoTitle: z.string(),
  metaDescription: z.string(),
})

type PostForm = z.infer<typeof postSchema>

export function PostEditor({ postId }: { postId?: string }) {
  const navigate = useNavigate()
  const { user } = useAuth()
  const { data: posts = [] } = usePosts({ mine: true })
  const existing = postId ? posts.find((p) => p.id === postId) : undefined
  const { data: mediaAssets = [] } = useMedia()
  const createPost = useCreatePost()
  const updatePost = useUpdatePost()

  const [body, setBody] = useState(existing?.bodyHtml ?? '')
  const [featuredImageId, setFeaturedImageId] = useState<string | null>(existing?.featuredImageId ?? null)
  const [pickerOpen, setPickerOpen] = useState(false)

  const { register, handleSubmit, control, reset } = useForm<PostForm>({
    resolver: zodResolver(postSchema),
    defaultValues: {
      title: existing?.title ?? '',
      pillar: existing?.pillar ?? 'tech',
      excerpt: existing?.excerpt ?? '',
      seoTitle: existing?.seoTitle ?? '',
      metaDescription: existing?.metaDescription ?? '',
    },
  })

  useEffect(() => {
    if (existing) {
      reset({
        title: existing.title,
        pillar: existing.pillar,
        excerpt: existing.excerpt,
        seoTitle: existing.seoTitle,
        metaDescription: existing.metaDescription,
      })
      setBody(existing.bodyHtml)
      setFeaturedImageId(existing.featuredImageId)
    }
  }, [existing, reset])

  const featuredImage = mediaAssets.find((m) => m.id === featuredImageId)

  function save(values: PostForm, status: 'draft' | 'pending_review' | 'published') {
    const payload = { ...values, bodyHtml: body, featuredImageId, status }
    if (existing) {
      updatePost.mutate({ id: existing.id, ...payload }, { onSuccess: () => navigate({ to: '/posts' }) })
    } else {
      createPost.mutate(payload, { onSuccess: () => navigate({ to: '/posts' }) })
    }
  }

  const isSaving = createPost.isPending || updatePost.isPending
  const canPublishDirectly = user?.role === 'editor' || user?.role === 'owner'

  return (
    <form className="mx-auto max-w-3xl space-y-6" onSubmit={(e) => e.preventDefault()}>
      <div className="flex items-center justify-between">
        <h1 className="font-heading text-2xl text-foreground">{existing ? 'Edit post' : 'New post'}</h1>
        <div className="flex gap-2">
          <Button type="button" variant="secondary" disabled={isSaving} onClick={handleSubmit((v) => save(v, 'draft'))}>
            Save draft
          </Button>
          <Button
            type="button"
            disabled={isSaving}
            onClick={handleSubmit((v) => save(v, canPublishDirectly ? 'published' : 'pending_review'))}
          >
            {canPublishDirectly ? 'Publish' : 'Submit for review'}
          </Button>
        </div>
      </div>

      <div className="space-y-2">
        <Label htmlFor="title">Title</Label>
        <Input id="title" {...register('title')} />
      </div>

      <div className="space-y-2">
        <Label>Pillar</Label>
        <Controller
          control={control}
          name="pillar"
          render={({ field }) => (
            <Select value={field.value} onValueChange={(v) => field.onChange(v as Pillar)}>
              <SelectTrigger><SelectValue /></SelectTrigger>
              <SelectContent>
                <SelectItem value="tech">Tech</SelectItem>
                <SelectItem value="social_psych">Social & Psychological</SelectItem>
                <SelectItem value="software_dev">Software Development</SelectItem>
              </SelectContent>
            </Select>
          )}
        />
      </div>

      <div className="space-y-2">
        <Label>Featured image</Label>
        {featuredImage && <img src={featuredImage.url} alt={featuredImage.filename} className="aspect-video w-full rounded-lg object-cover" />}
        <Dialog open={pickerOpen} onOpenChange={setPickerOpen}>
          <DialogTrigger asChild>
            <Button type="button" variant="secondary">Replace from library</Button>
          </DialogTrigger>
          <DialogContent className="max-w-4xl">
            <DialogTitle>Choose a featured image</DialogTitle>
            <MediaLibrary onSelect={(asset) => { setFeaturedImageId(asset.id); setPickerOpen(false) }} />
          </DialogContent>
        </Dialog>
      </div>

      <div className="space-y-2">
        <Label>Body</Label>
        <TipTapEditor value={body} onChange={setBody} />
      </div>

      <div className="space-y-2">
        <Label htmlFor="excerpt">Excerpt</Label>
        <Textarea id="excerpt" {...register('excerpt')} />
      </div>

      <div className="space-y-2">
        <Label htmlFor="seoTitle">SEO title</Label>
        <Input id="seoTitle" {...register('seoTitle')} />
      </div>

      <div className="space-y-2">
        <Label htmlFor="metaDescription">Meta description</Label>
        <Textarea id="metaDescription" {...register('metaDescription')} />
      </div>
    </form>
  )
}
