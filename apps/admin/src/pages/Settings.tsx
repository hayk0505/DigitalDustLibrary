import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useSettings, useUpdateSettings } from '@/lib/api/settings'
import { useActivity } from '@/lib/api/activity'
import { Avatar } from '@/components/shared/Avatar'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import { Button } from '@/components/ui/button'
import { formatRelativeTime } from '@/lib/formatting'

const settingsSchema = z.object({
  siteTitle: z.string().min(1, 'Site title is required'),
  tagline: z.string(),
  defaultMetaDescription: z.string(),
  linkedInUrl: z.string(),
  xUrl: z.string(),
})
type SettingsForm = z.infer<typeof settingsSchema>

export function Settings() {
  const { data: settings, isLoading } = useSettings()
  const updateSettings = useUpdateSettings()
  const { data: activity = [], isLoading: activityLoading } = useActivity()

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<SettingsForm>({
    resolver: zodResolver(settingsSchema),
    defaultValues: {
      siteTitle: '',
      tagline: '',
      defaultMetaDescription: '',
      linkedInUrl: '',
      xUrl: '',
    },
  })

  useEffect(() => {
    if (settings) {
      reset({
        siteTitle: settings.siteTitle,
        tagline: settings.tagline,
        defaultMetaDescription: settings.defaultMetaDescription,
        linkedInUrl: settings.linkedInUrl,
        xUrl: settings.xUrl,
      })
    }
  }, [settings, reset])

  function onSubmit(values: SettingsForm) {
    updateSettings.mutate(values)
  }

  if (isLoading) return <p className="text-muted-foreground">Loading…</p>

  return (
    <div className="mx-auto max-w-2xl space-y-10">
      <div>
        <div>
          <h1 className="font-heading text-2xl text-foreground">Settings</h1>
          <p className="mt-1 text-sm text-muted-foreground">Site-wide defaults used across the public blog.</p>
        </div>

        <form className="mt-6 space-y-4" onSubmit={handleSubmit(onSubmit)}>
          <div className="space-y-2">
            <Label htmlFor="siteTitle">Site title</Label>
            <Input id="siteTitle" {...register('siteTitle')} />
            {errors.siteTitle && <p className="text-sm text-destructive">{errors.siteTitle.message}</p>}
          </div>

          <div className="space-y-2">
            <Label htmlFor="tagline">Tagline</Label>
            <Input id="tagline" {...register('tagline')} />
          </div>

          <div className="space-y-2">
            <Label htmlFor="defaultMetaDescription">Default meta description</Label>
            <Textarea id="defaultMetaDescription" {...register('defaultMetaDescription')} />
          </div>

          <div className="space-y-2">
            <Label htmlFor="linkedInUrl">LinkedIn URL</Label>
            <Input id="linkedInUrl" {...register('linkedInUrl')} />
          </div>

          <div className="space-y-2">
            <Label htmlFor="xUrl">X URL</Label>
            <Input id="xUrl" {...register('xUrl')} />
          </div>

          <Button type="submit" disabled={updateSettings.isPending}>
            {updateSettings.isPending ? 'Saving…' : 'Save'}
          </Button>
        </form>
      </div>

      <div className="space-y-4">
        <h2 className="font-heading text-lg text-foreground">Activity</h2>
        {activityLoading ? (
          <p className="text-muted-foreground">Loading…</p>
        ) : (
          <div className="divide-y divide-border rounded-2xl border border-border bg-card">
            {activity.map((event) => (
              <div key={event.id} className="flex items-center gap-3 p-4">
                <Avatar name={event.actorName} size="sm" />
                <div className="min-w-0 flex-1">
                  <p className="text-sm text-foreground">
                    <span className="font-medium">{event.actorName}</span> {event.action}
                  </p>
                  <p className="text-xs text-muted-foreground">{formatRelativeTime(event.createdAt)}</p>
                </div>
              </div>
            ))}
            {activity.length === 0 && <p className="p-6 text-center text-muted-foreground">No activity yet.</p>}
          </div>
        )}
      </div>
    </div>
  )
}
