<script setup lang="ts">
import { computed } from 'vue'
import { withBase } from 'vitepress'
import { data } from './content-catalog.data'
import type { ContentType } from './content-catalog.data'

const props = defineProps<{ contentType?: ContentType }>()

const typeLabels: Record<ContentType, string> = {
  monster: 'モンスター',
  ability: 'Ability',
  art: 'Art',
  skill: 'Skill',
  evolution: 'Evolution'
}

const entries = computed(() => data.filter(entry =>
  entry.visibleInEncyclopedia &&
  (!props.contentType || entry.contentType === props.contentType)
))
</script>

<template>
  <table class="content-catalog">
    <thead>
      <tr>
        <th>Stable Content ID</th>
        <th>名称</th>
        <th v-if="!contentType">種別</th>
        <th>概要</th>
        <th>Source</th>
      </tr>
    </thead>
    <tbody>
      <tr v-for="entry in entries" :key="entry.contentId">
        <td><code>{{ entry.contentId }}</code></td>
        <td>
          <a v-if="entry.url" :href="withBase(entry.url)">{{ entry.title }}</a>
          <span v-else>{{ entry.title }}</span>
        </td>
        <td v-if="!contentType">{{ typeLabels[entry.contentType] }}</td>
        <td>{{ entry.description }}</td>
        <td>{{ entry.sourceKind }}</td>
      </tr>
    </tbody>
  </table>
</template>
