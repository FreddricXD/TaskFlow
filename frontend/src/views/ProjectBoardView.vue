<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import ActivityFeed from '@/components/ActivityFeed.vue'
import AnalyticsPanel from '@/components/AnalyticsPanel.vue'
import EmptyState from '@/components/EmptyState.vue'
import KanbanBoard from '@/components/KanbanBoard.vue'
import LoadingState from '@/components/LoadingState.vue'
import TaskFilters from '@/components/TaskFilters.vue'
import TaskModal from '@/components/TaskModal.vue'
import { useProjectStore } from '@/stores/projects'
import { useTaskStore } from '@/stores/tasks'
import type { TaskItem } from '@/types'

const route = useRoute()
const projects = useProjectStore()
const tasks = useTaskStore()

const projectId = computed(() => route.params.projectId as string)
const modalOpen = ref(false)
const selectedTask = ref<TaskItem | null>(null)

const newTask = reactive({
  title: '',
  description: '',
  priority: 'Medium',
  status: 'Todo',
})

async function loadBoard() {
  await Promise.all([projects.fetchProject(projectId.value), tasks.fetchTasks(projectId.value)])
}

onMounted(loadBoard)

watch(projectId, loadBoard)

watch(
  () => [tasks.filters.search, tasks.filters.status, tasks.filters.priority],
  () => tasks.fetchTasks(projectId.value),
)

function openCreateModal() {
  selectedTask.value = null
  newTask.title = ''
  newTask.description = ''
  newTask.priority = 'Medium'
  newTask.status = 'Todo'
  modalOpen.value = true
}

function openEditModal(task: TaskItem) {
  selectedTask.value = task
  modalOpen.value = true
}

async function handleSave(payload: Record<string, unknown>) {
  if (selectedTask.value) {
    await tasks.saveTask(projectId.value, selectedTask.value, payload)
  } else {
    await tasks.createTask(projectId.value, payload)
  }
  modalOpen.value = false
}
</script>

<template>
  <section class="page-header board-header">
    <div>
      <p class="eyebrow">Kanban</p>
      <h1>{{ projects.currentProject?.name ?? 'Project board' }}</h1>
      <p class="muted">{{ projects.currentProject?.description }}</p>
    </div>
    <button type="button" class="primary-button" @click="openCreateModal">New task</button>
  </section>

  <TaskFilters />

  <p v-if="tasks.conflictMessage" class="warning-banner" role="status">{{ tasks.conflictMessage }}</p>

  <LoadingState v-if="tasks.loading || projects.loading" label="Loading board" />

  <EmptyState
    v-else-if="!tasks.tasks.length"
    title="No tasks match your filters"
    description="Create a task or clear filters to populate the board."
  />

  <KanbanBoard
    v-else
    :project-id="projectId"
    @edit="openEditModal"
    @refresh="tasks.fetchTasks(projectId)"
  />

  <div class="insights-grid">
    <AnalyticsPanel :project-id="projectId" />
    <ActivityFeed :project-id="projectId" />
  </div>

  <TaskModal
    v-model:open="modalOpen"
    :task="selectedTask"
    :members="projects.currentProject?.members ?? []"
    :initial="newTask"
    @save="handleSave"
  />
</template>
