<script setup lang="ts">
import { computed } from 'vue'
import { withBase } from 'vitepress'
import { data } from './content-catalog.data'
import type { ContentType } from './content-catalog.data'

const props = defineProps<{ contentType?: ContentType }>()

const entries = computed(() => props.contentType
  ? data.filter(entry => entry.contentType === props.contentType)
  : data)
</script>

<template>
  <table class="content-catalog">
    <thead>
      <tr>
        <th>Stable Content ID</th>
        <th>名称</th>
        <th>Source</th>
      </tr>
    </thead>
    <tbody>
      <tr v-for="entry in entries" :key="entry.contentId">
        <td><code>{{ entry.contentId }}</code></td>
        <td><a :href="withBase(entry.url)">{{ entry.title }}</a></td>
        <td>{{ entry.sourceKind }}</td>
      </tr>
    </tbody>
  </table>
</template>
