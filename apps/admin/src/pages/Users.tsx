import { useState } from 'react'
import { useAuth } from '@/hooks/useAuth'
import { useUsers, useUpdateUser, useDeleteUser, useUserDeletionImpact } from '@/lib/api/users'
import { getUserStatusColors } from '@/lib/status'
import { Avatar } from '@/components/shared/Avatar'
import { FilterChips } from '@/components/shared/FilterChips'
import { Button } from '@/components/ui/button'
import { Dialog, DialogClose, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { cn } from '@/lib/utils'
import type { ManagedUser, Role } from '@/lib/types'

const ROLE_LABELS: Record<Role, string> = {
  author: 'Author',
  editor: 'Editor',
  owner: 'Owner',
}

const ROLE_FILTERS = [
  { value: 'all', label: 'All' },
  { value: 'author', label: 'Author' },
  { value: 'editor', label: 'Editor' },
  { value: 'owner', label: 'Owner' },
]

function Pill({ bg, fg, label }: { bg: string; fg: string; label: string }) {
  return (
    <span className={cn('inline-flex items-center rounded-full px-2.5 py-0.5 font-mono text-[11px] uppercase tracking-wide', bg, fg)}>
      {label}
    </span>
  )
}

function DeactivateDialog({
  user,
  open,
  onOpenChange,
}: {
  user: ManagedUser
  open: boolean
  onOpenChange: (open: boolean) => void
}) {
  const update = useUpdateUser()

  function handleConfirm() {
    update.mutate({ id: user.id, isActive: false })
    onOpenChange(false)
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Deactivate {user.name}?</DialogTitle>
          <DialogDescription>They'll be signed out immediately and won't be able to log back in until reactivated.</DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <DialogClose asChild>
            <Button variant="outline">Cancel</Button>
          </DialogClose>
          <Button variant="destructive" onClick={handleConfirm}>
            Deactivate
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}

function DeleteUserDialog({
  user,
  open,
  onOpenChange,
}: {
  user: ManagedUser
  open: boolean
  onOpenChange: (open: boolean) => void
}) {
  const deleteUser = useDeleteUser()
  const { data: impact, isPending: impactPending, isError: impactError } = useUserDeletionImpact(user.id, { enabled: open })
  const [confirmText, setConfirmText] = useState('')

  function handleConfirm() {
    deleteUser.mutate(user.id, {
      onSuccess: () => {
        setConfirmText('')
        onOpenChange(false)
      },
    })
  }

  const impactParts: string[] = []
  if (impact) {
    if (impact.postCount > 0) impactParts.push(`${impact.postCount} post(s)`)
    if (impact.mediaCount > 0) impactParts.push(`${impact.mediaCount} uploaded image(s)`)
    if (impact.reviewNoteCount > 0) {
      impactParts.push(`${impact.reviewNoteCount} review note(s) they wrote on other authors' posts`)
    }
    if (impact.affectedOtherPostCount > 0) {
      impactParts.push(`${impact.affectedOtherPostCount} other author's post(s) will lose their featured image`)
    }
  }

  let impactSentence: string
  if (impactPending) {
    impactSentence = 'Checking what will be deleted…'
  } else if (impactError) {
    impactSentence = "Couldn't load the deletion impact — proceed with caution."
  } else {
    impactSentence = `This permanently deletes their account${
      impactParts.length > 0 ? `, along with ${impactParts.join(', ')}` : ''
    }. This cannot be undone.`
  }

  return (
    <Dialog
      open={open}
      onOpenChange={(next) => {
        onOpenChange(next)
        if (!next) setConfirmText('')
      }}
    >
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Delete {user.name}?</DialogTitle>
          <DialogDescription>{impactSentence}</DialogDescription>
        </DialogHeader>
        <div className="space-y-2">
          <Label htmlFor="confirm-name">Type "{user.name}" to confirm</Label>
          <Input id="confirm-name" value={confirmText} onChange={(e) => setConfirmText(e.target.value)} />
        </div>
        <DialogFooter>
          <DialogClose asChild>
            <Button variant="outline">Cancel</Button>
          </DialogClose>
          <Button
            variant="destructive"
            disabled={confirmText !== user.name || impactPending || deleteUser.isPending}
            onClick={handleConfirm}
          >
            {deleteUser.isPending ? 'Deleting…' : 'Delete'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}

function UserRowActions({ user, isSelf }: { user: ManagedUser; isSelf: boolean }) {
  const update = useUpdateUser()
  const [confirmOpen, setConfirmOpen] = useState(false)
  const [deleteOpen, setDeleteOpen] = useState(false)

  if (!user.isActive) {
    return (
      <div className="flex gap-2">
        <Button size="sm" variant="secondary" onClick={() => update.mutate({ id: user.id, isActive: true })}>
          Reactivate
        </Button>
        <span title={isSelf ? "You can't delete your own account" : undefined} className="inline-flex">
          <Button size="sm" variant="destructive" disabled={isSelf} onClick={() => setDeleteOpen(true)}>
            Delete
          </Button>
        </span>
        <DeleteUserDialog user={user} open={deleteOpen} onOpenChange={setDeleteOpen} />
      </div>
    )
  }

  return (
    <div className="flex gap-2">
      <span title={isSelf ? "You can't deactivate your own account" : undefined} className="inline-flex">
        <Button size="sm" variant="destructive" disabled={isSelf} onClick={() => setConfirmOpen(true)}>
          Deactivate
        </Button>
      </span>
      <span title={isSelf ? "You can't delete your own account" : undefined} className="inline-flex">
        <Button size="sm" variant="destructive" disabled={isSelf} onClick={() => setDeleteOpen(true)}>
          Delete
        </Button>
      </span>
      <DeactivateDialog user={user} open={confirmOpen} onOpenChange={setConfirmOpen} />
      <DeleteUserDialog user={user} open={deleteOpen} onOpenChange={setDeleteOpen} />
    </div>
  )
}

export function Users() {
  const { user: currentUser } = useAuth()
  const { data: users = [], isLoading } = useUsers()
  const update = useUpdateUser()
  const [roleFilter, setRoleFilter] = useState('all')

  const filtered = roleFilter === 'all' ? users : users.filter((u) => u.role === roleFilter)

  return (
    <div className="space-y-6">
      <div>
        <h1 className="font-heading text-2xl text-foreground">Users & Roles</h1>
        <p className="mt-1 text-sm text-muted-foreground">
          Change a user's role, deactivate their account, or permanently delete it. You can't change your own role, deactivate yourself, or delete yourself.
        </p>
      </div>

      <FilterChips options={ROLE_FILTERS} value={roleFilter} onChange={setRoleFilter} />

      {isLoading ? (
        <p className="text-muted-foreground">Loading…</p>
      ) : (
        <div className="overflow-x-auto rounded-2xl border border-border bg-card">
          <table className="w-full min-w-[720px] text-sm">
            <thead>
              <tr className="border-b border-border text-left text-xs uppercase tracking-wide text-muted-foreground">
                <th className="px-4 py-3 font-normal">User</th>
                <th className="px-4 py-3 font-normal">Role</th>
                <th className="px-4 py-3 font-normal">Joined</th>
                <th className="px-4 py-3 font-normal">Status</th>
                <th className="px-4 py-3"><span className="sr-only">Actions</span></th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              {filtered.map((user) => {
                const isSelf = user.id === currentUser?.id
                const status = getUserStatusColors(user.isActive)
                return (
                  <tr key={user.id}>
                    <td className="px-4 py-3">
                      <div className="flex items-center gap-3">
                        <Avatar name={user.name} size="sm" />
                        <div>
                          <p className="text-foreground">{user.name}</p>
                          <p className="text-xs text-muted-foreground">{user.email}</p>
                        </div>
                      </div>
                    </td>
                    <td className="px-4 py-3">
                      <select
                        className="h-8 cursor-pointer rounded-md border border-input bg-transparent px-2 text-sm shadow-xs disabled:cursor-not-allowed disabled:opacity-50 dark:bg-input/30"
                        value={user.role}
                        disabled={isSelf}
                        title={isSelf ? "You can't change your own role" : undefined}
                        onChange={(e) => update.mutate({ id: user.id, role: e.target.value as Role })}
                      >
                        {(Object.keys(ROLE_LABELS) as Role[]).map((role) => (
                          <option key={role} value={role} className="text-black">
                            {ROLE_LABELS[role]}
                          </option>
                        ))}
                      </select>
                    </td>
                    <td className="px-4 py-3 text-foreground">
                      {new Date(user.createdAt).toLocaleDateString(undefined, {
                        year: 'numeric',
                        month: 'short',
                        day: 'numeric',
                      })}
                    </td>
                    <td className="px-4 py-3"><Pill {...status} /></td>
                    <td className="px-4 py-3">
                      <div className="flex justify-end">
                        <UserRowActions user={user} isSelf={isSelf} />
                      </div>
                    </td>
                  </tr>
                )
              })}
              {filtered.length === 0 && (
                <tr>
                  <td colSpan={5} className="px-4 py-6 text-center text-muted-foreground">
                    No users match this filter.
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
