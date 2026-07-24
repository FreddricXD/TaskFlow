import { defineStore } from 'pinia'
import {
  createTask,
  deleteTask,
  getTasks,
  moveTask,
  updateTask,
} from '@/services/taskflow'
import { getApiError } from '@/services/api'
import type { BoardStatus, TaskItem } from '@/types'

interface TaskFilters {
  search: string
  status: string
  priority: string
}

export const useTaskStore = defineStore('tasks', {
  state: () => ({
    tasks: [] as TaskItem[],
    loading: false,
    saving: false,
    error: '',
    conflictMessage: '',
    filters: {
      search: '',
      status: '',
      priority: '',
    } as TaskFilters,
  }),
  getters: {
    tasksByStatus: (state) => {
      return state.tasks.reduce<Record<BoardStatus, TaskItem[]>>(
        (acc, task) => {
          acc[task.status].push(task)
          return acc
        },
        { Todo: [], InProgress: [], Review: [], Done: [] },
      )
    },
  },
  actions: {
    setFilter(key: keyof TaskFilters, value: string) {
      this.filters[key] = value
    },
    async fetchTasks(projectId: string) {
      this.loading = true
      this.error = ''
      try {
        this.tasks = await getTasks(projectId, {
          search: this.filters.search || undefined,
          status: this.filters.status || undefined,
          priority: this.filters.priority || undefined,
        })
      } catch {
        this.error = 'Unable to load tasks.'
      } finally {
        this.loading = false
      }
    },
    async createTask(projectId: string, payload: Record<string, unknown>) {
      this.saving = true
      try {
        const task = await createTask(projectId, payload)
        this.tasks.push(task)
      } finally {
        this.saving = false
      }
    },
    async saveTask(projectId: string, task: TaskItem, payload: Record<string, unknown>) {
      this.saving = true
      this.conflictMessage = ''
      try {
        const updated = await updateTask(projectId, task.id, payload)
        this.replaceTask(updated)
      } catch (error) {
        const apiError = getApiError(error)
        if (apiError.code === 'conflict') {
          this.conflictMessage = apiError.message
        }
        throw error
      } finally {
        this.saving = false
      }
    },
    async moveTaskOptimistic(projectId: string, taskId: string, status: BoardStatus, sortOrder: number) {
      const task = this.tasks.find((item) => item.id === taskId)
      if (!task) return

      const previous = { status: task.status, sortOrder: task.sortOrder, version: task.version }
      task.status = status
      task.sortOrder = sortOrder

      try {
        const updated = await moveTask(projectId, taskId, {
          status,
          sortOrder,
          version: previous.version,
        })
        this.replaceTask(updated)
      } catch (error) {
        task.status = previous.status
        task.sortOrder = previous.sortOrder
        const apiError = getApiError(error)
        if (apiError.code === 'conflict') {
          this.conflictMessage = apiError.message
          await this.fetchTasks(projectId)
        }
        throw error
      }
    },
    async removeTask(projectId: string, taskId: string) {
      await deleteTask(projectId, taskId)
      this.tasks = this.tasks.filter((task) => task.id !== taskId)
    },
    replaceTask(updated: TaskItem) {
      const index = this.tasks.findIndex((task) => task.id === updated.id)
      if (index >= 0) {
        this.tasks[index] = updated
      }
    },
    upsertTask(task: TaskItem) {
      this.replaceTask(task)
      if (!this.tasks.some((item) => item.id === task.id)) {
        this.tasks.push(task)
      }
    },
    removeTaskById(taskId: string) {
      this.tasks = this.tasks.filter((task) => task.id !== taskId)
    },
  },
})
