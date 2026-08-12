import { useEffect, useState } from 'react'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useCategories, useCreateCategory, useUpdateCategory, useDeleteCategory } from '@/lib/api/categories'
import { getVisibilityColors, getCategoryStateColors } from '@/lib/status'
import { slugify } from '@/lib/formatting'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import { Dialog, DialogContent, DialogTrigger, DialogTitle, DialogFooter } from '@/components/ui/dialog'
import { cn } from '@/lib/utils'
import type { Category } from '@/lib/types'

export const categorySchema = z.object({
  name: z.string().min(1, 'Name is required'),
  slug: z.string().min(1, 'Slug is required'),
  description: z.string().min(1, 'Description is required'),
  color: z.string().regex(/^#[0-9a-fA-F]{6}$/, 'Must be a hex color like #A27B5B'),
  // An untouched <input type="number"> reads back as '' on submit, not undefined.
  // z.coerce.number() runs before .optional() ever sees the value, and
  // Number('') === 0 — so without this preprocess, leaving Position blank
  // silently coerces to a defined 0 (not "no position given"), which then
  // defeats the `body.position ?? <append-to-end default>` fallback both here
  // and server-side, since 0 isn't nullish. Normalize '' (and null/undefined)
  // to undefined *before* coercion runs, so an actually-blank field round-trips
  // as "omitted" the way the field's own hint text ("Leave blank to add it
  // after every existing category") promises.
  position: z.preprocess(
    (val) => (val === '' || val === null || val === undefined ? undefined : val),
    z.coerce.number().int().optional(),
  ),
})
type CategoryForm = z.infer<typeof categorySchema>
// z.coerce.number() makes the schema's input type (what defaultValues/register
// accept) diverge from its output type (what a validated submit produces —
// position: number | undefined post-coercion). react-hook-form 7.55+'s useForm
// takes a 3rd generic for exactly this split; without it, zodResolver's inferred
// Resolver<Input, Context, Output> doesn't structurally match Resolver<CategoryForm,
// any, CategoryForm> and tsc rejects it.
type CategoryFormInput = z.input<typeof categorySchema>

function EditCategoryDialog({ category, open, onOpenChange }: { category: Category; open: boolean; onOpenChange: (open: boolean) => void }) {
  const update = useUpdateCategory()
  const { register, handleSubmit, control, reset, formState: { errors } } = useForm<CategoryFormInput, unknown, CategoryForm>({
    resolver: zodResolver(categorySchema),
    defaultValues: {
      name: category.name,
      slug: category.slug,
      description: category.description,
      color: category.color,
      position: category.position,
    },
  })

  // EditCategoryDialog is rendered unconditionally inside CategoryRowActions
  // (so its Dialog can animate open/closed), which means useForm's
  // defaultValues only get captured once, at first mount — not on every
  // reopen, and not when `category` itself changes underneath it (e.g. after
  // a save invalidates and refetches ['categories']). Without this, reopening
  // Edit shows stale pre-edit values, and saving again without touching every
  // field would silently PATCH the earlier edit back to its old value.
  useEffect(() => {
    if (open) {
      reset({
        name: category.name,
        slug: category.slug,
        description: category.description,
        color: category.color,
        position: category.position,
      })
    }
  }, [open, category, reset])

  function onSubmit(values: CategoryForm) {
    update.mutate({ id: category.id, ...values }, { onSuccess: () => onOpenChange(false) })
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogTitle>Edit category</DialogTitle>
        <form className="space-y-4" onSubmit={handleSubmit(onSubmit)}>
          <div className="space-y-2">
            <Label htmlFor="edit-name">Name</Label>
            <Input id="edit-name" {...register('name')} />
            {errors.name && <p className="text-sm text-destructive">{errors.name.message}</p>}
          </div>
          <div className="space-y-2">
            <Label htmlFor="edit-slug">Slug</Label>
            <Input id="edit-slug" {...register('slug')} />
            {errors.slug && <p className="text-sm text-destructive">{errors.slug.message}</p>}
          </div>
          <div className="space-y-2">
            <Label htmlFor="edit-description">Description</Label>
            <Textarea id="edit-description" {...register('description')} />
            {errors.description && <p className="text-sm text-destructive">{errors.description.message}</p>}
          </div>
          <div className="space-y-2">
            <Label htmlFor="edit-color">Color</Label>
            <div className="flex items-center gap-2">
              <Controller
                control={control}
                name="color"
                render={({ field }) => (
                  <input type="color" value={field.value} onChange={(e) => field.onChange(e.target.value)} className="h-9 w-9 rounded border border-input" />
                )}
              />
              <Input id="edit-color" {...register('color')} className="font-mono" />
            </div>
            {errors.color && <p className="text-sm text-destructive">{errors.color.message}</p>}
          </div>
          <div className="space-y-2">
            <Label htmlFor="edit-position">Position</Label>
            <Input id="edit-position" type="number" {...register('position')} />
            {errors.position && <p className="text-sm text-destructive">{errors.position.message}</p>}
            <p className="text-xs text-muted-foreground">Lower numbers show first on the blog homepage.</p>
          </div>
          <DialogFooter>
            <Button type="submit" disabled={update.isPending}>{update.isPending ? 'Saving…' : 'Save'}</Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}

function Pill({ bg, fg, label }: { bg: string; fg: string; label: string }) {
  return (
    <span className={cn('inline-flex items-center rounded-full px-2.5 py-0.5 font-mono text-[11px] uppercase tracking-wide', bg, fg)}>
      {label}
    </span>
  )
}

function CategoryRowActions({ category }: { category: Category }) {
  const update = useUpdateCategory()
  const remove = useDeleteCategory()
  const [editOpen, setEditOpen] = useState(false)

  if (category.isDeleted) {
    return (
      <div className="flex justify-end">
        <Button size="sm" variant="secondary" onClick={() => update.mutate({ id: category.id, isDeleted: false })}>
          Restore
        </Button>
      </div>
    )
  }

  const locked = category.postCount > 0

  function handleDelete() {
    if (!window.confirm(`Permanently delete "${category.name}"? This cannot be undone.`)) return
    remove.mutate(category.id)
  }

  return (
    <div className="flex items-center justify-end gap-2">
      <Button size="sm" variant="secondary" onClick={() => setEditOpen(true)}>Edit</Button>
      <EditCategoryDialog category={category} open={editOpen} onOpenChange={setEditOpen} />
      <Button
        size="sm"
        variant="secondary"
        onClick={() => update.mutate({ id: category.id, isVisible: !category.isVisible })}
      >
        {category.isVisible ? 'Visible' : 'Hidden'}
      </Button>
      <Button size="sm" variant="secondary" onClick={() => update.mutate({ id: category.id, isDeleted: true })}>
        Archive
      </Button>
      <span title={locked ? 'Reassign posts first' : undefined} className="inline-flex">
        <Button size="sm" variant="destructive" disabled={locked} onClick={handleDelete}>
          Delete
        </Button>
      </span>
    </div>
  )
}

export function Categories() {
  const { data: categories = [], isLoading } = useCategories()
  const createCategory = useCreateCategory()
  const [open, setOpen] = useState(false)
  const [slugTouched, setSlugTouched] = useState(false)

  const {
    register,
    handleSubmit,
    control,
    setValue,
    reset,
    formState: { errors },
  } = useForm<CategoryFormInput, unknown, CategoryForm>({
    resolver: zodResolver(categorySchema),
    defaultValues: { name: '', slug: '', description: '', color: '#A27B5B', position: undefined },
  })

  function onSubmit(values: CategoryForm) {
    createCategory.mutate(values, {
      onSuccess: () => {
        setOpen(false)
        reset({ name: '', slug: '', description: '', color: '#A27B5B', position: undefined })
        setSlugTouched(false)
      },
    })
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="font-heading text-2xl text-foreground">Categories</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            Hidden categories drop out of public nav; deleted ones leave existing posts intact and can be restored.
          </p>
        </div>
        <Dialog
          open={open}
          onOpenChange={(next) => {
            setOpen(next)
            if (!next) {
              reset({ name: '', slug: '', description: '', color: '#A27B5B', position: undefined })
              setSlugTouched(false)
            }
          }}
        >
          <DialogTrigger asChild>
            <Button>+ Add category</Button>
          </DialogTrigger>
          <DialogContent>
            <DialogTitle>Add category</DialogTitle>
            <form className="space-y-4" onSubmit={handleSubmit(onSubmit)}>
              <div className="space-y-2">
                <Label htmlFor="name">Name</Label>
                <Input
                  id="name"
                  {...register('name', {
                    onChange: (e) => {
                      if (!slugTouched) setValue('slug', slugify(e.target.value))
                    },
                  })}
                />
                {errors.name && <p className="text-sm text-destructive">{errors.name.message}</p>}
              </div>
              <div className="space-y-2">
                <Label htmlFor="slug">Slug</Label>
                <Input id="slug" {...register('slug', { onChange: () => setSlugTouched(true) })} />
                {errors.slug && <p className="text-sm text-destructive">{errors.slug.message}</p>}
              </div>
              <div className="space-y-2">
                <Label htmlFor="description">Description</Label>
                <Textarea id="description" {...register('description')} />
                {errors.description && <p className="text-sm text-destructive">{errors.description.message}</p>}
              </div>
              <div className="space-y-2">
                <Label htmlFor="color">Color</Label>
                <div className="flex items-center gap-2">
                  <Controller
                    control={control}
                    name="color"
                    render={({ field }) => (
                      <input type="color" value={field.value} onChange={(e) => field.onChange(e.target.value)} className="h-9 w-9 rounded border border-input" />
                    )}
                  />
                  <Input id="color" {...register('color')} className="font-mono" />
                </div>
                {errors.color && <p className="text-sm text-destructive">{errors.color.message}</p>}
              </div>
              <div className="space-y-2">
                <Label htmlFor="position">Position (optional)</Label>
                <Input id="position" type="number" {...register('position')} />
                {errors.position && <p className="text-sm text-destructive">{errors.position.message}</p>}
                <p className="text-xs text-muted-foreground">Leave blank to add it after every existing category.</p>
              </div>
              <DialogFooter>
                <Button type="submit" disabled={createCategory.isPending}>
                  {createCategory.isPending ? 'Adding…' : 'Add category'}
                </Button>
              </DialogFooter>
            </form>
          </DialogContent>
        </Dialog>
      </div>

      {isLoading ? (
        <p className="text-muted-foreground">Loading…</p>
      ) : (
        <div className="overflow-x-auto rounded-2xl border border-border bg-card">
          <table className="w-full min-w-[720px] text-sm">
            <thead>
              <tr className="border-b border-border text-left text-xs uppercase tracking-wide text-muted-foreground">
                <th className="px-4 py-3 font-normal">Category</th>
                <th className="px-4 py-3 font-normal">Posts</th>
                <th className="px-4 py-3 font-normal">Visibility</th>
                <th className="px-4 py-3 font-normal">State</th>
                <th className="px-4 py-3"><span className="sr-only">Actions</span></th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              {categories.map((category) => {
                const visibility = getVisibilityColors(category.isVisible)
                const state = getCategoryStateColors(category.isDeleted)
                return (
                  <tr key={category.id}>
                    <td className="px-4 py-3">
                      <div className="flex items-center gap-2">
                        <span className="inline-block size-3 rounded-full" style={{ backgroundColor: category.color }} />
                        <span className="text-foreground">{category.name}</span>
                      </div>
                      <p className="font-mono text-xs text-muted-foreground">/{category.slug}</p>
                    </td>
                    <td className="px-4 py-3 text-foreground">{category.postCount}</td>
                    <td className="px-4 py-3"><Pill {...visibility} /></td>
                    <td className="px-4 py-3"><Pill {...state} /></td>
                    <td className="px-4 py-3"><CategoryRowActions category={category} /></td>
                  </tr>
                )
              })}
              {categories.length === 0 && (
                <tr>
                  <td colSpan={5} className="px-4 py-6 text-center text-muted-foreground">
                    No categories yet.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}
