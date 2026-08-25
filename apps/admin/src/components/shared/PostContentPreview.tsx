
export function PostContentPreview({
  tags,
  excerpt,
  featuredImageUrl,
  bodyHtml,
}: {
  tags: { id: string; name: string }[]
  excerpt: string
  featuredImageUrl?: string
  bodyHtml: string
}) {
  return (
    <div className="space-y-4">
      {tags.length > 0 && (
        <div className="flex flex-wrap gap-2">
          {tags.map((tag) => (
            <span key={tag.id} className="rounded-full bg-secondary px-2.5 py-0.5 text-xs text-secondary-foreground">
              {tag.name}
            </span>
          ))}
        </div>
      )}
      {excerpt && <p className="text-muted-foreground italic">{excerpt}</p>}
      {featuredImageUrl && (
        <img src={featuredImageUrl} alt="" className="aspect-video w-full rounded-lg object-cover" />
      )}
      <div className="prose dark:prose-invert max-w-none" dangerouslySetInnerHTML={{ __html: bodyHtml }} />
    </div>
  )
}
