import { useEffect, useState } from 'react'
import { EditorContent, useEditor, type Editor } from '@tiptap/react'
import StarterKit from '@tiptap/starter-kit'
import Link from '@tiptap/extension-link'
import Image from '@tiptap/extension-image'
import { cn } from '@/lib/utils'
import { Dialog, DialogContent, DialogTitle } from '@/components/ui/dialog'
import { MediaLibrary } from '@/pages/MediaLibrary'
import type { MediaAsset } from '@/lib/types'

interface ToolbarButtonConfig {
  label: string
  isActive: (editor: Editor) => boolean
  run: (editor: Editor) => void
}

const TOOLBAR_BUTTONS: ToolbarButtonConfig[] = [
  { label: 'Bold', isActive: (e) => e.isActive('bold'), run: (e) => e.chain().focus().toggleBold().run() },
  { label: 'Italic', isActive: (e) => e.isActive('italic'), run: (e) => e.chain().focus().toggleItalic().run() },
  { label: 'Strike', isActive: (e) => e.isActive('strike'), run: (e) => e.chain().focus().toggleStrike().run() },
  { label: 'H1', isActive: (e) => e.isActive('heading', { level: 1 }), run: (e) => e.chain().focus().toggleHeading({ level: 1 }).run() },
  { label: 'H2', isActive: (e) => e.isActive('heading', { level: 2 }), run: (e) => e.chain().focus().toggleHeading({ level: 2 }).run() },
  { label: 'Quote', isActive: (e) => e.isActive('blockquote'), run: (e) => e.chain().focus().toggleBlockquote().run() },
  { label: 'Bullet list', isActive: (e) => e.isActive('bulletList'), run: (e) => e.chain().focus().toggleBulletList().run() },
  { label: 'Numbered list', isActive: (e) => e.isActive('orderedList'), run: (e) => e.chain().focus().toggleOrderedList().run() },
  {
    label: 'Link',
    isActive: (e) => e.isActive('link'),
    run: (e) => {
      const url = window.prompt('URL')
      if (url) e.chain().focus().setLink({ href: url }).run()
    },
  },
  { label: 'Code', isActive: (e) => e.isActive('codeBlock'), run: (e) => e.chain().focus().toggleCodeBlock().run() },
]

export function TipTapEditor({ value, onChange }: { value: string; onChange: (html: string) => void }) {
  const [imagePickerOpen, setImagePickerOpen] = useState(false)
  const editor = useEditor({
    extensions: [StarterKit, Link.configure({ openOnClick: false }), Image],
    content: value,
    editorProps: {
      attributes: {
        class: 'prose prose-sm min-h-[420px] max-w-none p-4 text-foreground focus:outline-none',
      },
    },
    onUpdate: ({ editor }) => onChange(editor.getHTML()),
  })

  // Keep the editor in sync with external `value` changes that happen after
  // mount (e.g. an async post fetch resolving post-construction on a cold
  // navigation). Skip when `value` already matches the editor's own content,
  // since that's the echo of our own `onUpdate` firing from user typing —
  // re-applying it here would fight the user's cursor/selection every keystroke.
  useEffect(() => {
    if (!editor) return
    const current = editor.getHTML()
    if (value !== current) {
      editor.commands.setContent(value, { emitUpdate: false })
    }
  }, [value, editor])

  if (!editor) return null

  function insertImage(asset: MediaAsset) {
    editor.chain().focus().setImage({ src: asset.url, alt: asset.filename }).run()
    setImagePickerOpen(false)
  }

  return (
    <div className="rounded-2xl border border-border bg-card">
      <div className="sticky top-0 z-10 flex flex-wrap gap-1 rounded-t-2xl border-b border-border bg-card p-2">
        {TOOLBAR_BUTTONS.map((button) => (
          <button
            key={button.label}
            type="button"
            onClick={() => button.run(editor)}
            className={cn('rounded-md px-2 py-1 text-xs text-muted-foreground hover:bg-accent hover:text-foreground', button.isActive(editor) && 'bg-accent text-foreground')}
          >
            {button.label}
          </button>
        ))}
        <button
          type="button"
          onClick={() => setImagePickerOpen(true)}
          className={cn('rounded-md px-2 py-1 text-xs text-muted-foreground hover:bg-accent hover:text-foreground', editor.isActive('image') && 'bg-accent text-foreground')}
        >
          Image
        </button>
      </div>
      <EditorContent editor={editor} />
      <Dialog open={imagePickerOpen} onOpenChange={setImagePickerOpen}>
        <DialogContent className="max-w-4xl">
          <DialogTitle>Insert image</DialogTitle>
          <MediaLibrary onSelect={insertImage} />
        </DialogContent>
      </Dialog>
    </div>
  )
}
