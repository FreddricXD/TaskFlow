<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { getActivities } from '@/services/taskflow'
import type { ActivityItem } from '@/types'
import LoadingState from '@/components/LoadingState.vue'

const props = defineProps<{ projectId: string }>()

const items = ref<ActivityItem[]>([])
const loading = ref(true)

async function load() {
  loading.value = true
  try {
    items.value = await getActivities(props.projectId)
  } finally {
    loading.value = false
  }
}

onMounted(load)

defineExpose({ refresh: load })
</script>

<template>
  <section class="panel">
    <header class="panel-header">
      <h2>Activity feed</h2>
    </header>

    <LoadingState v-if="loading" label="Loading activity" />

    <ul v-else class="activity-list">
      <li v-for="item in items" :key="item.id">
        <strong>{{ item.userName }}</strong>
        <span>{{ item.description }}</span>
        <time>{{ new Date(item.createdAt).toLocaleString() }}</time>
      </li>
    </ul>
  </section>
</template>
