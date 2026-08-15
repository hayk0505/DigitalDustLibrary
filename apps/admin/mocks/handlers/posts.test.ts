import { describe, expect, it, afterEach } from 'vitest'
import { applyPostPatch } from './posts'
import { posts } from '../fixtures/posts'

// `applyPostPatch` is the exact mutation logic the PATCH /api/posts/:id MSW
// handler delegates to (see posts.ts) — tested directly here rather than via
// a live setupServer()+fetch() round trip, since that isn't reliable in this
// repo's current msw@2.15.0 + vitest/jsdom setup (confirmed via a minimal
// repro: even a trivial `http.get('/hello', ...)` handler never intercepted
// a matching `fetch()` call in this environment). This still exercises the
// real production code path, just without the (broken) network layer.
describe('applyPostPatch', () => {
  const originalPost1 = { ...posts.find((p) => p.id === 'post-1')! }
  const originalPost2 = { ...posts.find((p) => p.id === 'post-2')! }

  afterEach(() => {
    Object.assign(posts.find((p) => p.id === 'post-1')!, originalPost1)
    Object.assign(posts.find((p) => p.id === 'post-2')!, originalPost2)
  })

  it('re-resolves categoryName/categoryColor from the new categoryId, not just categoryId itself', () => {
    const updated = applyPostPatch('post-1', { categoryId: 'cat-dev' })

    expect(updated?.categoryId).toBe('cat-dev')
    expect(updated?.categoryName).toBe('Software Dev')
    expect(updated?.categoryColor).toBe('#4A6FBF')
  })

  it('leaves categoryName/categoryColor untouched when the patch has no categoryId', () => {
    const updated = applyPostPatch('post-2', { title: 'A retitled draft' })

    expect(updated?.title).toBe('A retitled draft')
    expect(updated?.categoryId).toBe('cat-social')
    expect(updated?.categoryName).toBe('Social · Psych')
    expect(updated?.categoryColor).toBe('#3F8F6A')
  })

  it('falls back to empty strings for a categoryId that matches no known category, rather than throwing', () => {
    const updated = applyPostPatch('post-1', { categoryId: 'cat-does-not-exist' })

    expect(updated?.categoryId).toBe('cat-does-not-exist')
    expect(updated?.categoryName).toBe('')
    expect(updated?.categoryColor).toBe('')
  })

  it('returns null for an unknown post id instead of throwing', () => {
    expect(applyPostPatch('post-does-not-exist', { categoryId: 'cat-dev' })).toBeNull()
  })
})
