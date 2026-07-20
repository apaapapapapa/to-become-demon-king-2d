<script setup lang="ts">
import { computed } from 'vue'
import { withBase } from 'vitepress'
import { data } from './content-catalog.data'

const props = defineProps<{ contentId: string }>()
const entriesById = new Map(data.map(entry => [entry.contentId, entry]))
const relatedEntries = computed(() => {
  const current = entriesById.get(props.contentId)
  return (current?.relatedContentIds ?? [])
    .map(contentId => entriesById.get(contentId))
    .filter(entry => entry !== undefined)
})
</script>

<template>
  <ul>
    <li v-for="entry in relatedEntries" :key="entry.contentId">
      <a :href="withBase(entry.url)">{{ entry.title }}</a>
      — <code>{{ entry.contentId }}</code>
    </li>
  </ul>
</template>
