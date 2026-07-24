import { HubConnectionBuilder, LogLevel, type HubConnection } from '@microsoft/signalr'
import type { ActivityItem, TaskItem } from '@/types'

export type BoardHubHandlers = {
  onTaskChanged?: (task: TaskItem) => void
  onTaskDeleted?: (taskId: string) => void
  onActivityCreated?: (activity: ActivityItem) => void
  onAnalyticsChanged?: () => void
}

export async function createBoardConnection(projectId: string, handlers: BoardHubHandlers) {
  const token = localStorage.getItem('taskflow_token') ?? ''
  const connection = new HubConnectionBuilder()
    .withUrl(`/hubs/taskboard?access_token=${encodeURIComponent(token)}`)
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Information)
    .build()

  if (handlers.onTaskChanged) {
    connection.on('TaskChanged', handlers.onTaskChanged)
  }

  if (handlers.onTaskDeleted) {
    connection.on('TaskDeleted', handlers.onTaskDeleted)
  }

  if (handlers.onActivityCreated) {
    connection.on('ActivityCreated', handlers.onActivityCreated)
  }

  if (handlers.onAnalyticsChanged) {
    connection.on('AnalyticsChanged', handlers.onAnalyticsChanged)
  }

  await connection.start()
  await connection.invoke('JoinProject', projectId)

  return connection
}

export async function disconnectBoard(connection: HubConnection | null, projectId: string) {
  if (!connection) return

  try {
    await connection.invoke('LeaveProject', projectId)
    await connection.stop()
  } catch {
    // Connection may already be closed during view teardown.
  }
}
