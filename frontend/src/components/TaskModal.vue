<script setup lang="ts">
import { computed, reactive, watch } from 'vue'
import type { ProjectMember, TaskItem } from '@/types'

const props = defineProps<{
  open: boolean
  task: TaskItem | null
  members: ProjectMember[]
  initial: { title: string; description: string; priority: string; status: string }
}>()

const emit = defineEmits<{
  'update:open': [boolean]
  save: [Record<string, unknown>]
}>()

const form = reactive({
  title: '',
  description: '',
  status: 'Todo',
  priority: 'Medium',
  assigneeId: '',
  dueDate: '',
  labels: '',
})

const isEditing = computed(() => Boolean(props.task))

watch(
  () => [props.open, props.task, props.initial],
  () => {
    if (!props.open) return
    if (props.task) {
      form.title = props.task.title
      form.description = props.task.description
      form.status = props.task.status
      form.priority = props.task.priority
      form.assigneeId = props.task.assigneeId ?? ''
      form.dueDate = props.task.dueDate ? props.task.dueDate.slice(0, 10) : ''
      form.labels = props.task.labels.map((label) => label.name).join(', ')
    } else {
      form.title = props.initial.title
      form.description = props.initial.description
      form.status = props.initial.status
      form.priority = props.initial.priority
      form.assigneeId = ''
      form.dueDate = ''
      form.labels = ''
    }
  },
  { immediate: true, deep: true },
)

function close() {
  emit('update:open', false)
}

function submit() {
  const payload: Record<string, unknown> = {
    title: form.title,
    description: form.description,
    status: form.status,
    priority: form.priority,
    assigneeId: form.assigneeId || null,
    dueDate: form.dueDate ? new Date(form.dueDate).toISOString() : null,
    labels: form.labels
      .split(',')
      .map((label) => label.trim())
      .filter(Boolean),
  }

  if (props.task) {
    payload.sortOrder = props.task.sortOrder
    payload.version = props.task.version
  }

  emit('save', payload)
}
</script>

<template>
  <div v-if="open" class="modal-backdrop" @click.self="close">
    <dialog open class="modal" aria-labelledby="task-modal-title">
      <header>
        <h2 id="task-modal-title">{{ isEditing ? 'Edit task' : 'Create task' }}</h2>
        <button type="button" class="ghost-button" @click="close">Close</button>
      </header>

      <form @submit.prevent="submit">
        <label>
          Title
          <input v-model="form.title" required />
        </label>

        <label>
          Description
          <textarea v-model="form.description" rows="4" />
        </label>

        <div class="form-grid">
          <label>
            Status
            <select v-model="form.status">
              <option value="Todo">To Do</option>
              <option value="InProgress">In Progress</option>
              <option value="Review">Review</option>
              <option value="Done">Done</option>
            </select>
          </label>

          <label>
            Priority
            <select v-model="form.priority">
              <option value="Low">Low</option>
              <option value="Medium">Medium</option>
              <option value="High">High</option>
              <option value="Critical">Critical</option>
            </select>
          </label>
        </div>

        <div class="form-grid">
          <label>
            Assignee
            <select v-model="form.assigneeId">
              <option value="">Unassigned</option>
              <option v-for="member in members" :key="member.id" :value="member.userId">
                {{ member.displayName }}
              </option>
            </select>
          </label>

          <label>
            Due date
            <input v-model="form.dueDate" type="date" />
          </label>
        </div>

        <label>
          Labels
          <input v-model="form.labels" placeholder="design, backend, urgent" />
        </label>

        <footer>
          <button type="button" class="ghost-button" @click="close">Cancel</button>
          <button type="submit" class="primary-button">{{ isEditing ? 'Save changes' : 'Create task' }}</button>
        </footer>
      </form>
    </dialog>
  </div>
</template>
