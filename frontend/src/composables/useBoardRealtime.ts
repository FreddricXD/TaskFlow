import { onMounted, onUnmounted, ref } from 'vue'
import type { HubConnection } from '@microsoft/signalr'
import { createBoardConnection, disconnectBoard } from '@/services/boardHub'
import { useTaskStore } from '@/stores/tasks'
import type { ActivityItem } from '@/types'

export function useBoardRealtime(projectId: () => string, callbacks?: {
  onActivity?: (activity: ActivityItem) => void
  onAnalytics?: () => void
}) {
  const tasks = useTaskStore()
  const connected = ref(false)
  let connection: HubConnection | null = null

  onMounted(async () => {
    connection = await createBoardConnection(projectId(), {
      onTaskChanged: (task) => tasks.upsertTask(task),
      onTaskDeleted: (taskId) => tasks.removeTaskById(taskId),
      onActivityCreated: (activity) => callbacks?.onActivity?.(activity),
      onAnalyticsChanged: () => callbacks?.onAnalytics?.(),
    })
    connected.value = true
  })

  onUnmounted(async () => {
    connected.value = false
    await disconnectBoard(connection, projectId())
    connection = null
  })

  return { connected }
}
