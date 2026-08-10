import { useState } from 'react'
import { useApplications, useApproveApplication, useRejectApplication } from '@/lib/api/applications'
import { getApplicationStatusColors } from '@/lib/status'
import { formatRelativeTime } from '@/lib/formatting'
import { Avatar } from '@/components/shared/Avatar'
import { Card } from '@/components/shared/Card'
import { FilterChips } from '@/components/shared/FilterChips'
import { Button } from '@/components/ui/button'
import { Dialog, DialogClose, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog'
import { cn } from '@/lib/utils'
import type { AuthorApplication } from '@/lib/types'

const STATUS_FILTERS = [
  { value: 'all', label: 'All' },
  { value: 'pending', label: 'Pending' },
  { value: 'approved', label: 'Approved' },
  { value: 'rejected', label: 'Rejected' },
]

function Pill({ bg, fg, label }: { bg: string; fg: string; label: string }) {
  return (
    <span className={cn('inline-flex items-center rounded-full px-2.5 py-0.5 font-mono text-[11px] uppercase tracking-wide', bg, fg)}>
      {label}
    </span>
  )
}

function ApproveDialog({
  application,
  open,
  onOpenChange,
}: {
  application: AuthorApplication
  open: boolean
  onOpenChange: (open: boolean) => void
}) {
  const approve = useApproveApplication()

  function handleConfirm() {
    approve.mutate(application.id)
    onOpenChange(false)
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Approve {application.name}?</DialogTitle>
          <DialogDescription>This creates their account and emails them an invite link.</DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <DialogClose asChild>
            <Button variant="outline">Cancel</Button>
          </DialogClose>
          <Button onClick={handleConfirm}>Approve</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}

function RejectDialog({
  application,
  open,
  onOpenChange,
}: {
  application: AuthorApplication
  open: boolean
  onOpenChange: (open: boolean) => void
}) {
  const reject = useRejectApplication()

  function handleConfirm() {
    reject.mutate(application.id)
    onOpenChange(false)
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Reject {application.name}'s application?</DialogTitle>
          <DialogDescription>They'll be notified by email. This can't be undone.</DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <DialogClose asChild>
            <Button variant="outline">Cancel</Button>
          </DialogClose>
          <Button variant="destructive" onClick={handleConfirm}>
            Reject
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}

function ApplicationCard({ application }: { application: AuthorApplication }) {
  const status = getApplicationStatusColors(application.status)
  const [approveOpen, setApproveOpen] = useState(false)
  const [rejectOpen, setRejectOpen] = useState(false)

  return (
    <Card className="space-y-3">
      <div className="flex items-start justify-between gap-4">
        <div className="flex items-center gap-3">
          <Avatar name={application.name} size="sm" />
          <div>
            <p className="text-foreground">{application.name}</p>
            <p className="text-xs text-muted-foreground">{application.email}</p>
          </div>
        </div>
        <Pill {...status} />
      </div>
      <p className="text-xs text-muted-foreground">Submitted {formatRelativeTime(application.submittedAt)}</p>
      <p className="text-sm text-foreground">{application.pitch}</p>
      {application.status === 'pending' ? (
        <div className="flex justify-end gap-2">
          <Button variant="destructive" onClick={() => setRejectOpen(true)}>
            Reject
          </Button>
          <Button onClick={() => setApproveOpen(true)}>Approve</Button>
        </div>
      ) : (
        application.reviewedAt && (
          <p className="text-xs text-muted-foreground">
            {application.status === 'approved' ? 'Approved' : 'Rejected'} {formatRelativeTime(application.reviewedAt)}
          </p>
        )
      )}
      <ApproveDialog application={application} open={approveOpen} onOpenChange={setApproveOpen} />
      <RejectDialog application={application} open={rejectOpen} onOpenChange={setRejectOpen} />
    </Card>
  )
}

export function Applications() {
  const { data: applications = [], isLoading } = useApplications()
  const [statusFilter, setStatusFilter] = useState('pending')

  const filtered = statusFilter === 'all' ? applications : applications.filter((a) => a.status === statusFilter)

  return (
    <div className="space-y-6">
      <div>
        <h1 className="font-heading text-2xl text-foreground">Applications</h1>
        <p className="mt-1 text-sm text-muted-foreground">
          Review author applications. Approving creates their account and emails an invite link; rejecting notifies them by email.
        </p>
      </div>

      <FilterChips options={STATUS_FILTERS} value={statusFilter} onChange={setStatusFilter} />

      {isLoading ? (
        <p className="text-muted-foreground">Loading…</p>
      ) : (
        <div className="space-y-4">
          {filtered.map((application) => (
            <ApplicationCard key={application.id} application={application} />
          ))}
          {filtered.length === 0 && <p className="text-muted-foreground">No applications match this filter.</p>}
        </div>
      )}
    </div>
  )
}
