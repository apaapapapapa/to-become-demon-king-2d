import { createContentLoader } from 'vitepress'

export type ContentType = 'monster' | 'art' | 'skill' | 'evolution'

export interface ContentCatalogEntry {
  contentId: string
  title: string
  contentType: ContentType
  status: string
  url: string
  relatedContentIds: string[]
}

const validTypes = new Set<ContentType>(['monster', 'art', 'skill', 'evolution'])

export default createContentLoader<ContentCatalogEntry[]>('database/**/*.md', {
  transform(pages) {
    const entries = pages
      .filter(({ frontmatter }) => frontmatter.contentId)
      .map(({ url, frontmatter }) => {
        const entry: ContentCatalogEntry = {
          contentId: String(frontmatter.contentId),
          title: String(frontmatter.title ?? ''),
          contentType: frontmatter.contentType as ContentType,
          status: String(frontmatter.status ?? ''),
          url,
          relatedContentIds: Array.isArray(frontmatter.relatedContentIds)
            ? frontmatter.relatedContentIds.map(String)
            : []
        }

        if (!entry.title || !entry.status || !validTypes.has(entry.contentType)) {
          throw new Error(`Content metadata is incomplete: ${url}`)
        }

        return entry
      })

    const entriesById = new Map<string, ContentCatalogEntry>()
    for (const entry of entries) {
      if (entriesById.has(entry.contentId)) {
        throw new Error(`Duplicate Stable Content ID: ${entry.contentId}`)
      }

      entriesById.set(entry.contentId, entry)
    }

    for (const entry of entries) {
      for (const relatedId of entry.relatedContentIds) {
        const related = entriesById.get(relatedId)
        if (!related) {
          throw new Error(`Unknown related Content ID: ${entry.contentId} -> ${relatedId}`)
        }

        if (!related.relatedContentIds.includes(entry.contentId)) {
          throw new Error(`Content relation is not reciprocal: ${entry.contentId} -> ${relatedId}`)
        }
      }
    }

    return entries.sort((left, right) => left.contentId.localeCompare(right.contentId))
  }
})
