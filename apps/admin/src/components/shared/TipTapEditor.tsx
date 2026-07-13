import { EditorContent, useEditor, type Editor } from '@tiptap/react'
import StarterKit from '@tiptap/starter-kit'
import Link from '@tiptap/extension-link'
import Image from '@tiptap/extension-image'
import { cn } from '@/lib/utils'

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
  {
    label: 'Image',
    isActive: () => false,
    run: (e) => {
      const url = window.prompt('Image URL')
      if (url) e.chain().focus().setImage({ src: url }).run()
    },
  },
  { label: 'Code', isActive: (e) => e.isActive('codeBlock'), run: (e) => e.chain().focus().toggleCodeBlock().run() },
]

export function TipTapEditor({ value, onChange }: { value: string; onChange: (html: string) => void }) {
  const editor = useEditor({
    extensions: [StarterKit, Link.configure({ openOnClick: false }), Image],
    content: value,
    onUpdate: ({ editor }) => onChange(editor.getHTML()),
  })

  if (!editor) return null

  return (
    <div className="rounded-2xl border border-border bg-card">
      <div className="flex flex-wrap gap-1 border-b border-border p-2">
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
      </div>
      <EditorContent editor={editor} className="prose prose-sm max-w-none p-4 text-foreground focus:outline-none" />
    </div>
  )
}
