<script setup lang="ts">
import { computed } from 'vue'
import { useData, withBase } from 'vitepress'
import { data } from './content-catalog.data'

const props = defineProps<{ contentId?: string }>()
const { frontmatter } = useData()
const entriesById = new Map(data.map(entry => [entry.contentId, entry]))

const currentEntry = computed(() => {
  if (props.contentId) {
    return entriesById.get(props.contentId)
  }

  const runtimeSource = String(frontmatter.value.runtimeSource ?? '')
  if (runtimeSource) {
    return data.find(entry => entry.runtimeSource === runtimeSource)
  }

  const contentId = String(frontmatter.value.contentId ?? '')
  return contentId ? entriesById.get(contentId) : undefined
})

const relatedEntries = computed(() => (currentEntry.value?.relatedContentIds ?? [])
  .map(contentId => entriesById.get(contentId))
  .filter(entry => entry !== undefined))
</script>

<template>
  <ul>
    <li v-for="entry in relatedEntries" :key="entry.contentId">
      <a :href="withBase(entry.url)">{{ entry.title }}</a>
      — <code>{{ entry.contentId }}</code>
    </li>
  </ul>
</template>
