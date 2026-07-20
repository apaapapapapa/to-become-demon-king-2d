<script setup lang="ts">
import { computed } from 'vue'
import { useData } from 'vitepress'
import { data } from './content-catalog.data'

const { frontmatter } = useData()

const entry = computed(() => {
  const runtimeSource = String(frontmatter.value.runtimeSource ?? '')
  if (!runtimeSource) {
    return undefined
  }

  return data.find(item => item.runtimeSource === runtimeSource)
})
</script>

<template>
  <template v-if="entry">
    <h1>{{ entry.title }}</h1>
    <p>
      <code>{{ entry.contentId }}</code>
      · {{ entry.sourceKind }}
    </p>
    <p v-if="entry.description">{{ entry.description }}</p>
    <template v-if="entry.encyclopediaDescription">
      <h2>図鑑解説</h2>
      <p>{{ entry.encyclopediaDescription }}</p>
    </template>
    <p v-if="entry.runtimeSource">
      Runtime Source: <code>{{ entry.runtimeSource }}</code>
    </p>
  </template>
</template>
