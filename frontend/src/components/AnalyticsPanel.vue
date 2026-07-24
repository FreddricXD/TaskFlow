<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { getAnalytics } from '@/services/taskflow'
import { STATUS_LABELS, type AnalyticsData, type BoardStatus } from '@/types'
import LoadingState from '@/components/LoadingState.vue'

const props = defineProps<{ projectId: string }>()

const analytics = ref<AnalyticsData | null>(null)
const loading = ref(true)

const totalTasks = computed(() =>
  analytics.value?.statusDistribution.reduce((total, item) => total + item.count, 0) ?? 0,
)
const completedTasks = computed(
  () => analytics.value?.statusDistribution.find((item) => item.status === 'Done')?.count ?? 0,
)
const activeTasks = computed(() => Math.max(totalTasks.value - completedTasks.value, 0))
const completionRate = computed(() =>
  totalTasks.value ? Math.round((completedTasks.value / totalTasks.value) * 100) : 0,
)
const maxTrend = computed(() =>
  Math.max(...(analytics.value?.completionTrend.map((point) => point.completed) ?? [1]), 1),
)
const completionRing = computed(() => ({
  background: `conic-gradient(var(--brand) 0 ${completionRate.value}%, var(--bar-track) ${completionRate.value}% 100%)`,
}))

function statusLabel(status: string) {
  return STATUS_LABELS[status as BoardStatus] ?? status
}

function trendHeight(completed: number) {
  return `${Math.max((completed / maxTrend.value) * 100, completed ? 12 : 4)}%`
}

function formatTrendDate(value: string) {
  return new Intl.DateTimeFormat(undefined, { weekday: 'short' }).format(new Date(`${value}T00:00:00`))
}

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
  <section class="panel analytics-panel">
    <header class="panel-header analytics-header">
      <div>
        <p class="eyebrow">Performance</p>
        <h2>Project analytics</h2>
      </div>
      <span v-if="analytics" class="analytics-period">Last 7 days</span>
    </header>

    <LoadingState v-if="loading" label="Loading analytics" />

    <div v-else-if="analytics" class="analytics-content">
      <div class="analytics-kpis">
        <article class="analytics-kpi">
          <span>Total tasks</span>
          <strong>{{ totalTasks }}</strong>
          <small>Across this project</small>
        </article>
        <article class="analytics-kpi">
          <span>Active work</span>
          <strong>{{ activeTasks }}</strong>
          <small>Still in progress</small>
        </article>
        <article class="analytics-kpi" :class="{ 'analytics-kpi--warning': analytics.overdueCount > 0 }">
          <span>Overdue</span>
          <strong>{{ analytics.overdueCount }}</strong>
          <small>{{ analytics.overdueCount ? 'Needs attention' : 'Everything on track' }}</small>
        </article>
      </div>

      <div class="analytics-visuals">
        <article class="analytics-breakdown">
          <div>
            <h3>Completion</h3>
            <p>Overall delivery progress</p>
          </div>

          <div class="completion-layout">
            <div class="completion-ring" :style="completionRing">
              <div>
                <strong>{{ completionRate }}%</strong>
                <span>complete</span>
              </div>
            </div>

            <ul class="status-legend">
              <li v-for="item in analytics.statusDistribution" :key="item.status">
                <span class="status-dot" :class="`status-dot--${item.status.toLowerCase()}`" />
                <span>{{ statusLabel(item.status) }}</span>
                <strong>{{ item.count }}</strong>
              </li>
            </ul>
          </div>
        </article>

        <article class="analytics-trend">
          <div class="analytics-chart-heading">
            <div>
              <h3>Completed tasks</h3>
              <p>Daily delivery trend</p>
            </div>
            <span>{{ analytics.completionTrend.reduce((sum, point) => sum + point.completed, 0) }} this week</span>
          </div>

          <div class="trend-chart" aria-label="Completed tasks over the last seven days">
            <div v-for="point in analytics.completionTrend" :key="point.date" class="trend-column">
              <div class="trend-bar-area">
                <span class="trend-value">{{ point.completed }}</span>
                <span class="trend-bar" :style="{ height: trendHeight(point.completed) }" />
              </div>
              <small>{{ formatTrendDate(point.date) }}</small>
            </div>
          </div>
        </article>
      </div>
    </div>
  </section>
</template>
