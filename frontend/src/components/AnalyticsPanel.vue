<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { getAnalytics } from '@/services/taskflow'
import type { AnalyticsData } from '@/types'
import LoadingState from '@/components/LoadingState.vue'

const props = defineProps<{ projectId: string }>()

const analytics = ref<AnalyticsData | null>(null)
const loading = ref(true)

const maxCount = computed(() =>
  Math.max(...(analytics.value?.statusDistribution.map((item) => item.count) ?? [1]), 1),
)

async function load() {
  loading.value = true
  try {
    analytics.value = await getAnalytics(props.projectId)
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
      <h2>Project analytics</h2>
      <span v-if="analytics" class="metric-chip">{{ analytics.overdueCount }} overdue</span>
    </header>

    <LoadingState v-if="loading" label="Loading analytics" />

    <div v-else-if="analytics" class="analytics-grid">
      <div class="chart-block">
        <h3>Status distribution</h3>
        <div v-for="item in analytics.statusDistribution" :key="item.status" class="bar-row">
          <span>{{ item.status }}</span>
          <div class="bar-track">
            <div class="bar-fill" :style="{ width: `${(item.count / maxCount) * 100}%` }" />
          </div>
          <strong>{{ item.count }}</strong>
        </div>
      </div>

      <div class="chart-block">
        <h3>Completion trend</h3>
        <div class="trend-grid">
          <div v-for="point in analytics.completionTrend" :key="point.date" class="trend-point">
            <span>{{ point.completed }}</span>
            <small>{{ point.date.slice(5) }}</small>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>
