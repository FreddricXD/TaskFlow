<script setup lang="ts">
import { computed, ref } from 'vue'
import TaskCard from '@/components/TaskCard.vue'
import { useTaskStore } from '@/stores/tasks'
import { BOARD_COLUMNS, STATUS_LABELS, type BoardStatus, type TaskItem } from '@/types'

const props = defineProps<{ projectId: string }>()
const emit = defineEmits<{ edit: [TaskItem]; refresh: [] }>()

const tasks = useTaskStore()
const draggingTaskId = ref<string | null>(null)

const columns = computed(() =>
  BOARD_COLUMNS.map((status) => ({
    status,
    label: STATUS_LABELS[status],
    tasks: tasks.tasksByStatus[status].slice().sort((a, b) => a.sortOrder - b.sortOrder),
  })),
)

function onDragStart(task: TaskItem) {
  draggingTaskId.value = task.id
}

function onDragEnd() {
  draggingTaskId.value = null
}

async function onDrop(status: BoardStatus) {
  const taskId = draggingTaskId.value
  if (!taskId) return

  const columnTasks = tasks.tasksByStatus[status]
  await tasks.moveTaskOptimistic(props.projectId, taskId, status, columnTasks.length)
  emit('refresh')
}
</script>

<template>
  <section class="kanban-board" aria-label="Kanban board">
    <div
      v-for="column in columns"
      :key="column.status"
      class="kanban-column"
      @dragover.prevent
      @drop="onDrop(column.status)"
    >
      <header>
        <h2>{{ column.label }}</h2>
        <span>{{ column.tasks.length }}</span>
      </header>

      <TaskCard
        v-for="task in column.tasks"
        :key="task.id"
        :task="task"
        @dragstart="onDragStart(task)"
        @dragend="onDragEnd"
        @edit="emit('edit', $event)"
      />
    </div>
  </section>
</template>
