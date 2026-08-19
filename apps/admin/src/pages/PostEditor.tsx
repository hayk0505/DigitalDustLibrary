import { useEffect, useState } from 'react'
import { useNavigate } from '@tanstack/react-router'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { usePosts, useCreatePost, useUpdatePost } from '@/lib/api/posts'
import { useMedia } from '@/lib/api/media'
import { useCategories } from '@/lib/api/categories'
import { useAuth } from '@/hooks/useAuth'
import { TipTapEditor } from '@/components/shared/TipTapEditor'
import { TagInput, type TagOption } from '@/components/shared/TagInput'
import { useTags, useCreateTag } from '@/lib/api/tags'
import { MediaLibrary } from './MediaLibrary'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import { Button } from '@/components/ui/button'
import { Dialog, DialogContent, DialogTrigger, DialogTitle } from '@/components/ui/dialog'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'

const postSchema = z.object({
  title: z.string().min(1, 'Title is required'),
  categoryId: z.string().min(1, 'Category is required'),
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
  const { data: categories = [] } = useCategories()
  // useCategories() is shared with the admin's Categories management screen,
  // which legitimately needs to see everything (including soft-deleted
  // rows) — so filter locally here instead of inside the hook. Only
  // IsDeleted is excluded; hidden-but-not-deleted categories stay assignable
  // (e.g. staging drafts in a category ahead of its public launch).
  const assignableCategories = categories.filter((category) => !category.isDeleted)
  const { data: allTagsRaw = [] } = useTags()
  const allTags: TagOption[] = allTagsRaw.map((t) => ({ id: t.id, name: t.name }))
  const createTag = useCreateTag()
  const createPost = useCreatePost()
  const updatePost = useUpdatePost()

  const [body, setBody] = useState(existing?.bodyHtml ?? '')
  const [featuredImageId, setFeaturedImageId] = useState<string | null>(existing?.featuredImageId ?? null)
  const [pickerOpen, setPickerOpen] = useState(false)
  const [tags, setTags] = useState<TagOption[]>(existing?.tags.map((t) => ({ id: t.id, name: t.name })) ?? [])
  const [isResolvingTags, setIsResolvingTags] = useState(false)

  const { register, handleSubmit, control, reset, getValues, setValue } = useForm<PostForm>({
    resolver: zodResolver(postSchema),
    defaultValues: {
      title: existing?.title ?? '',
      categoryId: existing?.categoryId ?? assignableCategories[0]?.id ?? '',
      excerpt: existing?.excerpt ?? '',
      seoTitle: existing?.seoTitle ?? '',
      metaDescription: existing?.metaDescription ?? '',
    },
  })

  useEffect(() => {
    if (existing) {
      reset({
        title: existing.title,
        categoryId: existing.categoryId,
        excerpt: existing.excerpt,
        seoTitle: existing.seoTitle,
        metaDescription: existing.metaDescription,
      })
      setBody(existing.bodyHtml)
      setFeaturedImageId(existing.featuredImageId)
      setTags(existing.tags.map((t) => ({ id: t.id, name: t.name })))
    }
  }, [existing, reset])

  // `defaultValues` above is a snapshot React Hook Form reads once at mount —
  // it does not re-run when `categories` (an async query, `[]` on first
  // render) finishes loading. Without this, a brand-new post's `categoryId`
  // locks in as '' forever, which the zod schema then silently rejects on
  // every submit. Backfill reactively once categories arrive, but only for
  // the new-post case (the `existing` effect above already handles pre-fill
  // for edits) and only if the user hasn't already picked a category.
  useEffect(() => {
    if (!existing && assignableCategories.length > 0 && !getValues('categoryId')) {
      setValue('categoryId', assignableCategories[0].id)
    }
  }, [existing, assignableCategories, getValues, setValue])

  const featuredImage = mediaAssets.find((m) => m.id === featuredImageId)

  async function resolveTagIds(): Promise<string[]> {
    const resolved = await Promise.all(
      tags.map((tag) => (tag.isNew ? createTag.mutateAsync({ name: tag.name }) : Promise.resolve(tag))),
    )
    return resolved.map((tag) => tag.id)
  }

  async function save(values: PostForm, status: 'draft' | 'pending_review' | 'published') {
    setIsResolvingTags(true)
    try {
      const tagIds = await resolveTagIds()
      const payload = { ...values, bodyHtml: body, featuredImageId, status, tagIds }
      if (existing) {
        updatePost.mutate({ id: existing.id, ...payload }, { onSuccess: () => navigate({ to: '/posts' }) })
      } else {
        createPost.mutate(payload, { onSuccess: () => navigate({ to: '/posts' }) })
      }
    } finally {
      setIsResolvingTags(false)
    }
  }

  // `isResolvingTags` covers the window between clicking Save/Publish and
  // `createPost.mutate()`/`updatePost.mutate()` actually firing — during
  // that window `resolveTagIds()` may be awaiting a real `POST /api/tags`
  // round-trip for any freshly-typed tag, and neither `createPost.isPending`
  // nor `updatePost.isPending` flips true until after that resolves. Without
  // this, the buttons stay clickable for the whole tag-creation round-trip
  // and a second click before it finishes fires a second, independent
  // `save()` call — `POST /api/posts` has no idempotency guard, so that
  // creates two separate draft posts instead of one.
  const isSaving = createPost.isPending || updatePost.isPending || isResolvingTags
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
        <Label>Category</Label>
        <Controller
          control={control}
          name="categoryId"
          render={({ field }) => (
            <Select value={field.value} onValueChange={field.onChange}>
              <SelectTrigger><SelectValue /></SelectTrigger>
              <SelectContent>
                {assignableCategories.map((category) => (
                  <SelectItem key={category.id} value={category.id}>{category.name}</SelectItem>
                ))}
              </SelectContent>
            </Select>
          )}
        />
      </div>

      <div className="space-y-2">
        <Label>Tags</Label>
        <TagInput value={tags} onChange={setTags} allTags={allTags} />
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
