<script setup lang="ts">
import { PRIORITY_COLORS, STATUS_LABELS, type TaskItem } from '@/types'

defineProps<{ task: TaskItem }>()
defineEmits<{ edit: [TaskItem]; dragstart: [TaskItem]; dragend: [] }>()

function formatDate(value?: string | null) {
  if (!value) return 'No due date'
  return new Intl.DateTimeFormat(undefined, { month: 'short', day: 'numeric' }).format(new Date(value))
}
</script>

<template>
  <article
    class="task-card"
    draggable="true"
    tabindex="0"
    :aria-label="`Task ${task.title}`"
    @click="$emit('edit', task)"
    @keydown.enter.prevent="$emit('edit', task)"
    @dragstart="$emit('dragstart', task)"
    @dragend="$emit('dragend')"
  >
    <div class="task-card-top">
      <span class="priority-pill" :style="{ backgroundColor: PRIORITY_COLORS[task.priority] }">
        {{ task.priority }}
      </span>
      <span class="status-pill">{{ STATUS_LABELS[task.status] }}</span>
    </div>
    <h3>{{ task.title }}</h3>
    <p>{{ task.description }}</p>
    <div class="task-card-meta">
      <span>{{ task.assigneeName ?? 'Unassigned' }}</span>
      <span>{{ formatDate(task.dueDate) }}</span>
    </div>
    <div v-if="task.labels.length" class="label-row">
      <span v-for="label in task.labels" :key="label.id" class="label-chip">{{ label.name }}</span>
    </div>
  </article>
</template>
