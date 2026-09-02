import { StatusBadge } from './StatusBadge'
import type { Category } from '@/lib/types'

// Category count/names are never hardcoded here — the real list can grow
// past the four that exist today, so this reads live from `categories`.
function formatCategoryList(names: string[]): string {
  if (names.length === 0) return 'a category'
  if (names.length === 1) return names[0]
  if (names.length === 2) return `${names[0]} or ${names[1]}`
  return `${names.slice(0, -1).join(', ')}, or ${names[names.length - 1]}`
}

// Mirrors docs/legal/contributor-guidelines.md section-for-section (minus
// its own draft/implementation note, which isn't author-facing) rather than
// a condensed rewrite — keep this in sync if that file changes. Meant to be
// dropped inside a DialogContent (see PostEditor's Guidelines button/dialog)
// — no chrome of its own, just the content.
export function ContributorGuidelinesPanel({ categories }: { categories: Category[] }) {
  const categoryList = formatCategoryList(categories.map((c) => c.name))

  return (
    <div className="max-h-[70vh] space-y-5 overflow-y-auto text-sm text-foreground">
      <section>
        <h3 className="font-heading text-sm font-semibold">Welcome</h3>
        <p className="mt-1.5 text-muted-foreground">
          You're publishing under the Digital Dust Library name, alongside other authors. These are the
          rules of the road — read them once, keep them handy.
        </p>
      </section>

      <section>
        <h3 className="font-heading text-sm font-semibold">How publishing works here</h3>
        <p className="mt-1.5 text-muted-foreground">Every post moves through a simple pipeline:</p>
        <div className="mt-2 flex flex-wrap items-center gap-1.5">
          <StatusBadge status="draft" />
          <span className="text-muted-foreground" aria-hidden="true">→</span>
          <StatusBadge status="pending_review" />
          <span className="text-muted-foreground" aria-hidden="true">→</span>
          <StatusBadge status="published" />
        </div>
        <p className="mt-2 flex flex-wrap items-center gap-1.5 text-muted-foreground">
          or <StatusBadge status="changes_requested" />, sent back to you with reviewer notes if
          something needs fixing before it can go out.
        </p>
        <p className="mt-2 text-muted-foreground">
          As an Author, you can only submit and edit your own posts. An Editor or the Owner reviews
          everything before it goes live — either approving and publishing it, or sending it back with
          specific feedback. If you get Changes Requested, revise and resubmit; there's no limit on how
          many rounds that takes.
        </p>
      </section>

      <section>
        <h3 className="font-heading text-sm font-semibold">What to write</h3>
        <p className="mt-1.5 text-muted-foreground">
          Posts fall into one of our categories — {categoryList} — and one of two formats:
        </p>
        <ul className="mt-2 list-disc space-y-1.5 pl-4 text-muted-foreground">
          <li>
            <span className="font-medium text-foreground">Minis</span> — short, a single thought or
            observation, no minimum length requirement beyond &ldquo;actually finished.&rdquo;
          </li>
          <li>
            <span className="font-medium text-foreground">Shelf</span> — full long-form essays, the
            site&rsquo;s deep-dive format.
          </li>
        </ul>
        <p className="mt-2 text-muted-foreground">
          Pick whichever fits what you&rsquo;re writing; don&rsquo;t force a short idea into a long-form
          shape or vice versa.
        </p>
      </section>

      <section>
        <h3 className="font-heading text-sm font-semibold">Voice and quality bar</h3>
        <p className="mt-1.5 text-muted-foreground">
          Write like you mean it — well-researched, your own analysis, not regurgitated summary. If
          you&rsquo;re stating something as fact, it should hold up (cite a source in the text or link it
          if it&rsquo;s not common knowledge). Editors will send back anything that reads like filler or
          that makes claims you can&rsquo;t back up.
        </p>
      </section>

      <section>
        <h3 className="font-heading text-sm font-semibold">Images — read this carefully</h3>
        <p className="mt-1.5 text-muted-foreground">
          You need the right to publish any image you upload. That means one of:
        </p>
        <ul className="mt-2 list-disc space-y-1.5 pl-4 text-muted-foreground">
          <li>
            <span className="font-medium text-foreground">Your own original work</span> — a photo you
            took, a graphic you made.
          </li>
          <li>
            <span className="font-medium text-foreground">Public domain</span> images.
          </li>
          <li>
            <span className="font-medium text-foreground">Openly licensed stock photography</span> from
            sources like Unsplash, Pexels, Pixabay, or Wikimedia Commons — check the specific license on
            each image, since not all Wikimedia Commons images are cleared for this kind of use.
          </li>
          <li>
            <span className="font-medium text-foreground">Stock you&rsquo;ve properly licensed</span> — a
            paid license you personally hold covers you; a colleague&rsquo;s or a different project&rsquo;s
            license does not.
          </li>
        </ul>
        <p className="mt-2 text-muted-foreground">
          <span className="font-medium text-foreground">Not okay:</span> pulling an image from a Google
          Images search, another website, or social media without verifying you&rsquo;re actually allowed
          to use it. No watermark doesn&rsquo;t mean no copyright — most images online are protected
          whether or not they&rsquo;re marked.
        </p>
        <p className="mt-2 text-muted-foreground">
          If a license requires attribution, include the credit line in the post (caption or a note near
          the image).
        </p>
        <p className="mt-2 text-muted-foreground">
          <span className="font-medium text-foreground">
            AI-generated images are allowed but must be disclosed
          </span>{' '}
          — note in the post that an image was AI-generated and, if relevant, which tool. This is partly a
          transparency issue and partly because the copyright status of AI-generated images is still
          legally unsettled in a lot of jurisdictions — better to be upfront.
        </p>
        <p className="mt-2 text-muted-foreground">
          You don&rsquo;t need to resize or compress anything before uploading — the media library
          automatically optimizes images on upload (downscales large images, converts to a web-friendly
          format). Just upload the best-quality original you have.
        </p>
      </section>

      <section>
        <h3 className="font-heading text-sm font-semibold">Content standards</h3>
        <ul className="mt-1.5 list-disc space-y-1.5 pl-4 text-muted-foreground">
          <li>No plagiarism — your own words, your own analysis; direct quotes need attribution.</li>
          <li>No hate speech, harassment, or doxxing.</li>
          <li>No unverified claims presented as settled fact.</li>
          <li>
            If a piece was substantially AI-assisted in the writing (not just research), disclose that in
            the post.
          </li>
        </ul>
      </section>

      <section>
        <h3 className="font-heading text-sm font-semibold">Byline and bio</h3>
        <p className="mt-1.5 text-muted-foreground">
          Your name appears on every post you publish. Once the Authors page is live, your bio and links
          will show there too — you&rsquo;ll be able to set that up yourself.
        </p>
      </section>

      <section className="rounded-lg bg-accent p-3">
        <h3 className="font-heading text-sm font-semibold">Questions</h3>
        <p className="mt-1.5 text-muted-foreground">
          Reach out to an Editor or the Owner if anything here is unclear before you publish, not after.
        </p>
      </section>
    </div>
  )
}
