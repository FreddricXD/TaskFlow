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
    saveError: '',
    conflictMessage: '',
    filters: {
      search: '',
      status: '',
      priority: '',
    } as TaskFilters,
  }),
  getters: {
    tasksByStatus: (state) => {
      const seenTaskIds = new Set<string>()

      return state.tasks.reduce<Record<BoardStatus, TaskItem[]>>(
        (acc, task) => {
          if (seenTaskIds.has(task.id)) {
            return acc
          }

          seenTaskIds.add(task.id)
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
        const fetchedTasks = await getTasks(projectId, {
          search: this.filters.search || undefined,
          status: this.filters.status || undefined,
          priority: this.filters.priority || undefined,
        })
        this.tasks = fetchedTasks.filter(
          (task, index, allTasks) => allTasks.findIndex((candidate) => candidate.id === task.id) === index,
        )
      } catch {
        this.error = 'Unable to load tasks.'
      } finally {
        this.loading = false
      }
    },
    async createTask(projectId: string, payload: Record<string, unknown>) {
      this.saving = true
      this.saveError = ''
      try {
        const task = await createTask(projectId, payload)
        this.upsertTask(task)
        await this.fetchTasks(projectId)
      } catch (error) {
        this.saveError = getApiError(error).message
        throw error
      } finally {
        this.saving = false
      }
    },
    async saveTask(projectId: string, task: TaskItem, payload: Record<string, unknown>) {
      this.saving = true
      this.saveError = ''
      this.conflictMessage = ''
      try {
        const updated = await updateTask(projectId, task.id, payload)
        this.replaceTask(updated)
      } catch (error) {
        const apiError = getApiError(error)
        if (apiError.code === 'conflict') {
          this.conflictMessage = apiError.message
        }
        this.saveError = apiError.message
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
      this.saving = true
      this.saveError = ''
      try {
        await deleteTask(projectId, taskId)
        this.tasks = this.tasks.filter((task) => task.id !== taskId)
      } catch (error) {
        this.saveError = getApiError(error).message
        throw error
      } finally {
        this.saving = false
      }
    },
    replaceTask(updated: TaskItem) {
      const index = this.tasks.findIndex((task) => task.id === updated.id)
      if (index >= 0) {
        this.tasks[index] = updated
      }
    },
    upsertTask(task: TaskItem) {
      const index = this.tasks.findIndex((item) => item.id === task.id)
      if (index < 0) {
        this.tasks.push(task)
        return
      }

      this.tasks.splice(index, 1, task)
      this.tasks = this.tasks.filter((item, currentIndex) => item.id !== task.id || currentIndex === index)
    },
    removeTaskById(taskId: string) {
      this.tasks = this.tasks.filter((task) => task.id !== taskId)
    },
  },
})
