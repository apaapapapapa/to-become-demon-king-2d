import fs from 'node:fs'
import path from 'node:path'
import { fileURLToPath } from 'node:url'
import { createContentLoader } from 'vitepress'

export type ContentType = 'monster' | 'ability' | 'art' | 'skill' | 'evolution'
export type ContentSourceKind = 'Unity Definition' | 'Runtime Code' | 'Knowledge Base'

export interface ContentCatalogEntry {
  contentId: string
  title: string
  description: string
  encyclopediaDescription: string
  contentType: ContentType
  url?: string
  relatedContentIds: string[]
  sourceKind: ContentSourceKind
  runtimeSource?: string
  visibleInEncyclopedia: boolean
}

interface RuntimeContentRecord {
  contentId: string
  displayName?: string
  description: string
  encyclopediaDescription: string
  contentType: ContentType
  runtimeSource: string
  sourceKind: Exclude<ContentSourceKind, 'Knowledge Base'>
  relatedContentIds: string[]
  visibleInEncyclopedia: boolean
  guid?: string
}

interface PendingCatalogEntry extends ContentCatalogEntry {
  manualRelatedContentIds: string[]
  runtimeRelatedContentIds: string[]
}

const loaderDirectory = path.dirname(fileURLToPath(import.meta.url))
const repositoryRootCandidates = [
  process.cwd(),
  path.resolve(process.cwd(), '..'),
  path.resolve(loaderDirectory, '../../..')
]
const repositoryRoot = repositoryRootCandidates.find(candidate =>
  fs.existsSync(path.join(candidate, 'Assets')) &&
  fs.existsSync(path.join(candidate, 'docs'))
)

if (!repositoryRoot) {
  throw new Error('Repository root could not be resolved for the VitePress content loader.')
}

const gameplaySettingsRoot = path.join(
  repositoryRoot,
  'Assets/Resources/Settings/Gameplay'
)

function normalizeRepositoryPath(filePath: string): string {
  return path.relative(repositoryRoot, filePath).split(path.sep).join('/')
}

function readFilesRecursively(directory: string, extension: string): string[] {
  if (!fs.existsSync(directory)) {
    return []
  }

  return fs.readdirSync(directory, { withFileTypes: true }).flatMap(entry => {
    const entryPath = path.join(directory, entry.name)
    if (entry.isDirectory()) {
      return readFilesRecursively(entryPath, extension)
    }

    return entry.isFile() && entry.name.endsWith(extension) ? [entryPath] : []
  })
}

function stripQuotes(value: string): string {
  const trimmed = value.trim()
  if (
    (trimmed.startsWith('"') && trimmed.endsWith('"')) ||
    (trimmed.startsWith("'") && trimmed.endsWith("'"))
  ) {
    return trimmed.slice(1, -1)
  }

  return trimmed
}

function escapeRegExp(value: string): string {
  return value.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')
}

function extractYamlScalar(content: string, fieldName: string): string | undefined {
  const match = content.match(
    new RegExp(`^\\s*${escapeRegExp(fieldName)}:\\s*(.*?)\\s*$`, 'm')
  )
  if (!match) {
    return undefined
  }

  const value = stripQuotes(match[1])
  return value.length > 0 ? value : undefined
}

function extractYamlBoolean(
  content: string,
  fieldName: string,
  defaultValue: boolean
): boolean {
  const value = extractYamlScalar(content, fieldName)?.toLowerCase()
  if (value === undefined) {
    return defaultValue
  }

  if (value === '1' || value === 'true') {
    return true
  }

  if (value === '0' || value === 'false') {
    return false
  }

  throw new Error(`Invalid boolean value for ${fieldName}: ${value}`)
}

function extractCSharpString(content: string, fieldName: string): string | undefined {
  const match = content.match(
    new RegExp(`\\b${escapeRegExp(fieldName)}\\s*=\\s*"([^"]+)"`)
  )
  return match?.[1]
}

function extractStableContentIds(content: string): string[] {
  return Array.from(
    new Set(
      content.match(/\b(?:character|ability|art|skill|evolution)\.[A-Za-z0-9_.-]+/g) ?? []
    )
  )
}

function extractGuids(content: string): string[] {
  const guids: string[] = []
  for (const match of content.matchAll(/guid:\s*([0-9a-f]{32})/gi)) {
    guids.push(match[1].toLowerCase())
  }

  return Array.from(new Set(guids))
}

function readMetaGuid(assetPath: string): string | undefined {
  const metaPath = `${assetPath}.meta`
  if (!fs.existsSync(metaPath)) {
    return undefined
  }

  const match = fs.readFileSync(metaPath, 'utf8').match(/^guid:\s*([0-9a-f]{32})\s*$/mi)
  return match?.[1].toLowerCase()
}

function identifyUnityContent(
  content: string,
  runtimeSource: string,
  guid?: string
): RuntimeContentRecord | undefined {
  const definitions: Array<{
    idField: string
    type: ContentType
  }> = [
    { idField: 'characterId', type: 'monster' },
    { idField: 'abilityId', type: 'ability' },
    { idField: 'artId', type: 'art' },
    { idField: 'skillId', type: 'skill' },
    { idField: 'evolutionNodeId', type: 'evolution' }
  ]

  for (const definition of definitions) {
    const contentId = extractYamlScalar(content, definition.idField)
    if (!contentId) {
      continue
    }

    return {
      contentId,
      displayName: extractYamlScalar(content, 'displayName'),
      description: extractYamlScalar(content, 'description') ?? '',
      encyclopediaDescription: extractYamlScalar(content, 'encyclopediaDescription') ?? '',
      contentType: definition.type,
      runtimeSource,
      sourceKind: 'Unity Definition',
      relatedContentIds: extractStableContentIds(content).filter(id => id !== contentId),
      visibleInEncyclopedia: extractYamlBoolean(content, 'visibleInEncyclopedia', true),
      guid
    }
  }

  return undefined
}

function scanUnityContent(): Map<string, RuntimeContentRecord> {
  const recordsByPath = new Map<string, RuntimeContentRecord>()
  const recordsByGuid = new Map<string, RuntimeContentRecord>()
  const sourceTextByPath = new Map<string, string>()

  for (const assetPath of readFilesRecursively(gameplaySettingsRoot, '.asset')) {
    const content = fs.readFileSync(assetPath, 'utf8')
    const runtimeSource = normalizeRepositoryPath(assetPath)
    const record = identifyUnityContent(content, runtimeSource, readMetaGuid(assetPath))
    if (!record) {
      continue
    }

    recordsByPath.set(runtimeSource, record)
    sourceTextByPath.set(runtimeSource, content)
    if (record.guid) {
      recordsByGuid.set(record.guid, record)
    }
  }

  for (const record of recordsByPath.values()) {
    const content = sourceTextByPath.get(record.runtimeSource) ?? ''
    const relatedIds = new Set(record.relatedContentIds)

    for (const guid of extractGuids(content)) {
      const referenced = recordsByGuid.get(guid)
      if (referenced && referenced.contentId !== record.contentId) {
        relatedIds.add(referenced.contentId)
      }
    }

    record.relatedContentIds = Array.from(relatedIds)
  }

  return recordsByPath
}

const unityContentByPath = scanUnityContent()

function resolveRuntimeContent(runtimeSource: string): RuntimeContentRecord {
  const normalizedSource = runtimeSource.split('\\').join('/')
  const unityRecord = unityContentByPath.get(normalizedSource)
  if (unityRecord) {
    return unityRecord
  }

  const absolutePath = path.resolve(repositoryRoot, normalizedSource)
  const relativePath = path.relative(repositoryRoot, absolutePath)
  if (
    relativePath.startsWith('..') ||
    path.isAbsolute(relativePath) ||
    !fs.existsSync(absolutePath)
  ) {
    throw new Error(`Runtime source does not exist: ${runtimeSource}`)
  }

  const content = fs.readFileSync(absolutePath, 'utf8')
  if (normalizedSource.endsWith('.asset')) {
    const record = identifyUnityContent(content, normalizedSource, readMetaGuid(absolutePath))
    if (record) {
      return record
    }
  }

  if (normalizedSource.endsWith('.cs')) {
    const actorId = extractCSharpString(content, 'actorId')
    if (actorId) {
      return {
        contentId: actorId,
        description: '',
        encyclopediaDescription: '',
        contentType: 'monster',
        runtimeSource: normalizedSource,
        sourceKind: 'Runtime Code',
        relatedContentIds: [],
        visibleInEncyclopedia: false
      }
    }
  }

  throw new Error(`Runtime content metadata could not be resolved: ${runtimeSource}`)
}

function inferContentType(url: string): ContentType {
  if (url.includes('/database/monsters/')) return 'monster'
  if (url.includes('/database/abilities/')) return 'ability'
  if (url.includes('/database/arts/')) return 'art'
  if (url.includes('/database/skills/')) return 'skill'
  if (url.includes('/database/evolutions/')) return 'evolution'

  throw new Error(`Content type could not be inferred from URL: ${url}`)
}

function hasExpectedPrefix(contentId: string, contentType: ContentType): boolean {
  const prefixes: Record<ContentType, string> = {
    monster: 'character.',
    ability: 'ability.',
    art: 'art.',
    skill: 'skill.',
    evolution: 'evolution.'
  }
  return contentId.startsWith(prefixes[contentType])
}

function addUndirectedRelation(
  relations: Map<string, Set<string>>,
  leftId: string,
  rightId: string
): void {
  if (leftId === rightId) {
    return
  }

  relations.get(leftId)?.add(rightId)
  relations.get(rightId)?.add(leftId)
}

export default createContentLoader<ContentCatalogEntry[]>('database/**/*.md', {
  transform(pages) {
    const pendingEntries: PendingCatalogEntry[] = pages
      .filter(({ frontmatter }) => frontmatter.runtimeSource || frontmatter.contentId)
      .map(({ url, frontmatter }) => {
        const contentType = inferContentType(url)
        const runtimeSource = frontmatter.runtimeSource
          ? String(frontmatter.runtimeSource)
          : undefined
        const runtimeContent = runtimeSource
          ? resolveRuntimeContent(runtimeSource)
          : undefined

        if (frontmatter.contentType) {
          throw new Error(`contentType is derived from the database directory and must not be duplicated: ${url}`)
        }

        if (frontmatter.status) {
          throw new Error(`Runtime implementation status must not be duplicated in Markdown metadata: ${url}`)
        }

        if (runtimeContent && runtimeContent.contentType !== contentType) {
          throw new Error(
            `Runtime source type mismatch: ${url} -> ${runtimeContent.runtimeSource}`
          )
        }

        if (runtimeContent?.displayName && frontmatter.title) {
          throw new Error(
            `title duplicates Unity displayName; remove it from frontmatter: ${url}`
          )
        }

        const contentId = runtimeContent?.contentId ?? String(frontmatter.contentId ?? '')
        const title = runtimeContent?.displayName ?? String(frontmatter.title ?? '')

        if (!contentId || !title || !hasExpectedPrefix(contentId, contentType)) {
          throw new Error(`Content metadata is incomplete or invalid: ${url}`)
        }

        return {
          contentId,
          title,
          description: runtimeContent?.description ?? String(frontmatter.description ?? ''),
          encyclopediaDescription:
            runtimeContent?.encyclopediaDescription ??
            String(frontmatter.encyclopediaDescription ?? ''),
          contentType,
          url,
          relatedContentIds: [],
          sourceKind: runtimeContent?.sourceKind ?? 'Knowledge Base',
          runtimeSource,
          visibleInEncyclopedia:
            runtimeContent?.visibleInEncyclopedia ??
            Boolean(frontmatter.visibleInEncyclopedia ?? true),
          manualRelatedContentIds: Array.isArray(frontmatter.relatedContentIds)
            ? frontmatter.relatedContentIds.map(String)
            : [],
          runtimeRelatedContentIds: runtimeContent?.relatedContentIds ?? []
        }
      })

    const representedRuntimeSources = new Set(
      pendingEntries
        .map(entry => entry.runtimeSource)
        .filter((runtimeSource): runtimeSource is string => Boolean(runtimeSource))
    )

    for (const runtimeContent of unityContentByPath.values()) {
      if (representedRuntimeSources.has(runtimeContent.runtimeSource)) {
        continue
      }

      const title = runtimeContent.displayName ?? runtimeContent.contentId
      if (runtimeContent.visibleInEncyclopedia && !runtimeContent.displayName) {
        throw new Error(
          `Visible encyclopedia content requires displayName: ${runtimeContent.runtimeSource}`
        )
      }

      pendingEntries.push({
        contentId: runtimeContent.contentId,
        title,
        description: runtimeContent.description,
        encyclopediaDescription: runtimeContent.encyclopediaDescription,
        contentType: runtimeContent.contentType,
        relatedContentIds: [],
        sourceKind: runtimeContent.sourceKind,
        runtimeSource: runtimeContent.runtimeSource,
        visibleInEncyclopedia: runtimeContent.visibleInEncyclopedia,
        manualRelatedContentIds: [],
        runtimeRelatedContentIds: runtimeContent.relatedContentIds
      })
    }

    const entriesById = new Map<string, PendingCatalogEntry>()
    for (const entry of pendingEntries) {
      if (entriesById.has(entry.contentId)) {
        throw new Error(`Duplicate Stable Content ID: ${entry.contentId}`)
      }
      entriesById.set(entry.contentId, entry)
    }

    const relations = new Map<string, Set<string>>(
      pendingEntries.map(entry => [entry.contentId, new Set<string>()] as const)
    )

    for (const entry of pendingEntries) {
      for (const relatedId of entry.manualRelatedContentIds) {
        if (!entriesById.has(relatedId)) {
          throw new Error(`Unknown related Content ID: ${entry.contentId} -> ${relatedId}`)
        }
        addUndirectedRelation(relations, entry.contentId, relatedId)
      }

      for (const relatedId of entry.runtimeRelatedContentIds) {
        if (entriesById.has(relatedId)) {
          addUndirectedRelation(relations, entry.contentId, relatedId)
        }
      }
    }

    return pendingEntries
      .map(({ manualRelatedContentIds: _manual, runtimeRelatedContentIds: _runtime, ...entry }) => ({
        ...entry,
        relatedContentIds: Array.from(relations.get(entry.contentId) ?? []).sort()
      }))
      .sort((left, right) => left.contentId.localeCompare(right.contentId))
  }
})
