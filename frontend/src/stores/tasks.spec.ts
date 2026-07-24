import { createPinia, setActivePinia } from 'pinia'
import { beforeEach, describe, expect, it } from 'vitest'
import { useTaskStore } from '@/stores/tasks'
import type { TaskItem } from '@/types'

const task: TaskItem = {
  id: 'task-1',
  projectId: 'project-1',
  title: 'Test task',
  description: '',
  status: 'Todo',
  priority: 'Medium',
  assigneeId: null,
  assigneeName: null,
  dueDate: null,
  sortOrder: 0,
  version: 1,
  createdAt: new Date().toISOString(),
  updatedAt: new Date().toISOString(),
  labels: [],
}

describe('task store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  it('upserts the same realtime task only once', () => {
    const store = useTaskStore()

    store.upsertTask(task)
    store.upsertTask({ ...task, version: 2 })

    expect(store.tasks).toHaveLength(1)
    expect(store.tasks[0]?.version).toBe(2)
  })

  it('removes duplicate copies already in state', () => {
    const store = useTaskStore()
    store.tasks = [task, { ...task }]

    store.upsertTask({ ...task, status: 'Done', version: 2 })

    expect(store.tasks).toHaveLength(1)
    expect(store.tasks[0]?.status).toBe('Done')
  })

  it('never renders duplicate IDs in board columns', () => {
    const store = useTaskStore()
    store.tasks = [task, { ...task }]

    expect(store.tasksByStatus.Todo).toHaveLength(1)
  })
})
