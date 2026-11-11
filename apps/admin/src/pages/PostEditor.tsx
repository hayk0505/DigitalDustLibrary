import { useEffect, useState } from 'react'
import { useNavigate } from '@tanstack/react-router'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { usePosts, useCreatePost, useUpdatePost, useDeletePost } from '@/lib/api/posts'
import { useMedia } from '@/lib/api/media'
import { useCategories } from '@/lib/api/categories'
import { useAuth } from '@/hooks/useAuth'
import { TipTapEditor } from '@/components/shared/TipTapEditor'
import { TagInput, type TagOption } from '@/components/shared/TagInput'
import { PostContentPreview } from '@/components/shared/PostContentPreview'
import { LoadingOverlay } from '@/components/shared/LoadingOverlay'
import { ContributorGuidelinesPanel } from '@/components/shared/ContributorGuidelinesPanel'
import { CategoryTag } from '@/components/shared/CategoryTag'
import { Avatar } from '@/components/shared/Avatar'
import { useTags, useCreateTag } from '@/lib/api/tags'
import { MediaPickerDialog } from '@/components/shared/MediaPickerDialog'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import { Button } from '@/components/ui/button'
import { Dialog, DialogContent, DialogTitle, DialogDescription, DialogFooter } from '@/components/ui/dialog'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'

const postSchema = z.object({
  title: z.string().min(1, 'Title is required'),
  categoryId: z.string().min(1, 'Category is required'),
  format: z.enum(['mini', 'shelf']),
  excerpt: z.string(),
  seoTitle: z.string(),
  metaDescription: z.string(),
  source: z.string(),
})

type PostForm = z.infer<typeof postSchema>

export function PostEditor({ postId }: { postId?: string }) {
  const navigate = useNavigate()
  const { user } = useAuth()
  const isReviewer = user?.role === 'editor' || user?.role === 'owner'
  const { data: posts = [] } = usePosts({ mine: !isReviewer })
  const existing = postId ? posts.find((p) => p.id === postId) : undefined
  const { data: mediaAssets = [] } = useMedia()
  const { data: categories = [] } = useCategories()
  const assignableCategories = categories.filter((category) => !category.isDeleted)
  const { data: allTagsRaw = [] } = useTags()
  const allTags: TagOption[] = allTagsRaw.map((t) => ({ id: t.id, name: t.name }))
  const createTag = useCreateTag()
  const createPost = useCreatePost()
  const updatePost = useUpdatePost()
  const deletePost = useDeletePost()

  const [body, setBody] = useState(existing?.bodyHtml ?? '')
  const [featuredImageId, setFeaturedImageId] = useState<string | null>(existing?.featuredImageId ?? null)
  const [pickerOpen, setPickerOpen] = useState(false)
  const [tags, setTags] = useState<TagOption[]>(existing?.tags.map((t) => ({ id: t.id, name: t.name })) ?? [])
  const [isResolvingTags, setIsResolvingTags] = useState(false)
  const [confirmDeleteOpen, setConfirmDeleteOpen] = useState(false)
  const [previewSnapshot, setPreviewSnapshot] = useState<PostForm | null>(null)
  const [guidelinesOpen, setGuidelinesOpen] = useState(false)

  const { register, handleSubmit, control, reset, getValues, setValue } = useForm<PostForm>({
    resolver: zodResolver(postSchema),
    defaultValues: {
      title: existing?.title ?? '',
      categoryId: existing?.categoryId ?? assignableCategories[0]?.id ?? '',
      format: existing?.format ?? 'shelf',
      excerpt: existing?.excerpt ?? '',
      seoTitle: existing?.seoTitle ?? '',
      metaDescription: existing?.metaDescription ?? '',
      source: existing?.source ?? '',
    },
  })

  useEffect(() => {
    if (existing) {
      reset({
        title: existing.title,
        categoryId: existing.categoryId,
        format: existing.format,
        excerpt: existing.excerpt,
        seoTitle: existing.seoTitle,
        metaDescription: existing.metaDescription,
        source: existing.source ?? '',
      })
      setBody(existing.bodyHtml)
      setFeaturedImageId(existing.featuredImageId)
      setTags(existing.tags.map((t) => ({ id: t.id, name: t.name })))
    }
  }, [existing, reset])

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

  const isSaving = createPost.isPending || updatePost.isPending || isResolvingTags
  const canPublishDirectly = user?.role === 'editor' || user?.role === 'owner'

  function confirmDelete() {
    if (!existing) return
    deletePost.mutate(existing.id, { onSuccess: () => navigate({ to: '/posts' }) })
  }

  function cancel() {
    if (existing) {
      navigate({ to: '/posts/$postId', params: { postId: existing.id } })
    } else {
      navigate({ to: '/posts' })
    }
  }

  function openPreview() {
    setPreviewSnapshot(getValues())
  }

  const previewCategory = previewSnapshot
    ? assignableCategories.find((c) => c.id === previewSnapshot.categoryId)
    : undefined
  const previewAuthorName = existing?.authorName ?? user?.name ?? 'You'

  return (
    <form className="mx-auto max-w-3xl space-y-6" onSubmit={(e) => e.preventDefault()}>
      <div className="flex items-center justify-between">
        <h1 className="font-heading text-2xl text-foreground">{existing ? 'Edit post' : 'New post'}</h1>
        <div className="flex gap-2">
          <Button
            type="button"
            variant="outline"
            className="border-accent-text text-accent-text hover:bg-accent hover:text-accent-text"
            onClick={() => setGuidelinesOpen(true)}
          >
            Guidelines
          </Button>
          <Button type="button" variant="secondary" onClick={cancel}>
            Cancel
          </Button>
          <Button type="button" variant="secondary" onClick={openPreview}>
            Preview
          </Button>
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
          {existing && (
            <Button type="button" variant="destructive" onClick={() => setConfirmDeleteOpen(true)}>
              Delete post
            </Button>
          )}
        </div>
      </div>

      {existing && <LoadingOverlay show={deletePost.isPending} />}

      {existing && (
        <Dialog open={confirmDeleteOpen} onOpenChange={setConfirmDeleteOpen}>
          <DialogContent>
            <DialogTitle>Delete post</DialogTitle>
            <DialogDescription>
              Permanently delete "{existing.title}"? This cannot be undone.
            </DialogDescription>
            <DialogFooter>
              <Button variant="secondary" onClick={() => setConfirmDeleteOpen(false)}>Cancel</Button>
              <Button variant="destructive" disabled={deletePost.isPending} onClick={confirmDelete}>
                {deletePost.isPending ? 'Deleting…' : 'Delete'}
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      )}

      <Dialog open={previewSnapshot !== null} onOpenChange={(open) => !open && setPreviewSnapshot(null)}>
        <DialogContent className="max-w-3xl">
          <DialogTitle>Preview</DialogTitle>
          {previewSnapshot && (
            <div className="max-h-[70vh] space-y-4 overflow-y-auto">
              <CategoryTag name={previewCategory?.name ?? ''} color={previewCategory?.color ?? '#A27B5B'} />
              <h1 className="font-heading text-2xl text-foreground">{previewSnapshot.title || 'Untitled draft'}</h1>
              <div className="flex items-center gap-3">
                <Avatar name={previewAuthorName} size="sm" />
                <div className="text-sm text-muted-foreground">{previewAuthorName}</div>
              </div>
              <PostContentPreview
                tags={tags}
                excerpt={previewSnapshot.excerpt}
                featuredImageUrl={featuredImage?.url}
                bodyHtml={body}
              />
            </div>
          )}
        </DialogContent>
      </Dialog>

      <Dialog open={guidelinesOpen} onOpenChange={setGuidelinesOpen}>
        <DialogContent className="max-w-2xl">
          <DialogTitle>Digital Dust Library — Contributor Guidelines</DialogTitle>
          <ContributorGuidelinesPanel categories={assignableCategories} />
        </DialogContent>
      </Dialog>

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
              <SelectContent position="popper" align="start">
                {assignableCategories.map((category) => (
                  <SelectItem key={category.id} value={category.id}>{category.name}</SelectItem>
                ))}
              </SelectContent>
            </Select>
          )}
        />
      </div>

      <div className="space-y-2">
        <Label>Type</Label>
        <Controller
          control={control}
          name="format"
          render={({ field }) => (
            <Select value={field.value} onValueChange={field.onChange}>
              <SelectTrigger><SelectValue /></SelectTrigger>
              <SelectContent position="popper" align="start">
                <SelectItem value="shelf">Shelf</SelectItem>
                <SelectItem value="mini">Mini</SelectItem>
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
        <Button type="button" variant="secondary" onClick={() => setPickerOpen(true)}>Replace from library</Button>
        <MediaPickerDialog
          open={pickerOpen}
          onOpenChange={setPickerOpen}
          title="Choose a featured image"
          onSelect={(asset) => { setFeaturedImageId(asset.id); setPickerOpen(false) }}
          // If the current featured image gets deleted, drop its id so the
          // next save doesn't send a dangling FK (Posts.FeaturedImageId).
          onDeleted={(asset) => setFeaturedImageId((current) => (current === asset.id ? null : current))}
        />
      </div>

      <div className="space-y-2">
        <Label htmlFor="source">Source</Label>
        <Input id="source" placeholder="e.g. NASA press release" {...register('source')} />
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
