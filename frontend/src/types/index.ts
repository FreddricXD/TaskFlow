export interface User {
  id: string
  email: string
  displayName: string
}

export interface AuthResponse {
  token: string
  user: User
}

export interface Project {
  id: string
  name: string
  description: string
  ownerId: string
  ownerName: string
  createdAt: string
  taskCount: number
  memberCount: number
}

export interface ProjectMember {
  id: string
  userId: string
  displayName: string
  email: string
  role: string
}

export interface ProjectDetail extends Omit<Project, 'taskCount' | 'memberCount'> {
  members: ProjectMember[]
}

export type BoardStatus = 'Todo' | 'InProgress' | 'Review' | 'Done'
export type TaskPriority = 'Low' | 'Medium' | 'High' | 'Critical'

export interface TaskLabel {
  id: string
  name: string
  color: string
}

export interface TaskItem {
  id: string
  projectId: string
  title: string
  description: string
  status: BoardStatus
  priority: TaskPriority
  assigneeId?: string | null
  assigneeName?: string | null
  dueDate?: string | null
  sortOrder: number
  version: number
  createdAt: string
  updatedAt: string
  labels: TaskLabel[]
}

export interface ActivityItem {
  id: string
  projectId: string
  userId: string
  userName: string
  entityType: string
  entityId: string
  action: string
  description: string
  createdAt: string
}

export interface AnalyticsData {
  statusDistribution: { status: string; count: number }[]
  overdueCount: number
  completionTrend: { date: string; completed: number }[]
}

export interface ApiError {
  message: string
  code?: string
}

export const BOARD_COLUMNS: BoardStatus[] = ['Todo', 'InProgress', 'Review', 'Done']

export const STATUS_LABELS: Record<BoardStatus, string> = {
  Todo: 'To Do',
  InProgress: 'In Progress',
  Review: 'Review',
  Done: 'Done',
}

export const PRIORITY_COLORS: Record<TaskPriority, string> = {
  Low: '#64748b',
  Medium: '#0ea5e9',
  High: '#f97316',
  Critical: '#ef4444',
}
