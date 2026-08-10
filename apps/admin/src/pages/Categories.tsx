import { useState } from 'react'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useCategories, useCreateCategory, useUpdateCategory, useDeleteCategory } from '@/lib/api/categories'
import { getVisibilityColors, getCategoryStateColors } from '@/lib/status'
import { slugify } from '@/lib/formatting'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Checkbox } from '@/components/ui/checkbox'
import { Dialog, DialogContent, DialogTrigger, DialogTitle, DialogFooter } from '@/components/ui/dialog'
import { cn } from '@/lib/utils'
import type { Category } from '@/lib/types'

const categorySchema = z.object({
  name: z.string().min(1, 'Name is required'),
  slug: z.string().min(1, 'Slug is required'),
  isPillar: z.boolean(),
})
type CategoryForm = z.infer<typeof categorySchema>

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
  } = useForm<CategoryForm>({
    resolver: zodResolver(categorySchema),
    defaultValues: { name: '', slug: '', isPillar: false },
  })

  function onSubmit(values: CategoryForm) {
    createCategory.mutate(values, {
      onSuccess: () => {
        setOpen(false)
        reset({ name: '', slug: '', isPillar: false })
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
            Pillars are the default three. Hidden categories drop out of public nav; deleted ones leave existing posts intact and can be restored.
          </p>
        </div>
        <Dialog
          open={open}
          onOpenChange={(next) => {
            setOpen(next)
            if (!next) {
              reset({ name: '', slug: '', isPillar: false })
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
              <div className="flex items-center gap-2">
                <Controller
                  control={control}
                  name="isPillar"
                  render={({ field }) => (
                    <Checkbox
                      id="isPillar"
                      checked={field.value}
                      onCheckedChange={(checked) => field.onChange(checked === true)}
                    />
                  )}
                />
                <Label htmlFor="isPillar">This is one of the three core pillars</Label>
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
                        <span className="text-foreground">{category.name}</span>
                        {category.isPillar && (
                          <span className="inline-flex items-center rounded-full border border-border px-2 py-0.5 font-mono text-[10px] uppercase tracking-wide text-muted-foreground">
                            Pillar
                          </span>
                        )}
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
