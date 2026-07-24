import api from './api'
import type {
  ActivityItem,
  AnalyticsData,
  AuthResponse,
  Project,
  ProjectDetail,
  TaskItem,
  User,
} from '@/types'

export async function login(email: string, password: string) {
  const { data } = await api.post<AuthResponse>('/auth/login', { email, password })
  return data
}

export async function register(displayName: string, email: string, password: string) {
  const { data } = await api.post<AuthResponse>('/auth/register', { displayName, email, password })
  return data
}

export async function getCurrentUser() {
  const { data } = await api.get<User>('/auth/me')
  return data
}

export async function getProjects() {
  const { data } = await api.get<Project[]>('/projects')
  return data
}

export async function getProject(projectId: string) {
  const { data } = await api.get<ProjectDetail>(`/projects/${projectId}`)
  return data
}

export async function createProject(name: string, description: string) {
  const { data } = await api.post<ProjectDetail>('/projects', { name, description })
  return data
}

export async function getTasks(projectId: string, filters?: { search?: string; status?: string; priority?: string }) {
  const { data } = await api.get<TaskItem[]>(`/projects/${projectId}/tasks`, { params: filters })
  return data
}

export async function createTask(projectId: string, payload: Record<string, unknown>) {
  const { data } = await api.post<TaskItem>(`/projects/${projectId}/tasks`, payload)
  return data
}

export async function updateTask(projectId: string, taskId: string, payload: Record<string, unknown>) {
  const { data } = await api.put<TaskItem>(`/projects/${projectId}/tasks/${taskId}`, payload)
  return data
}

export async function moveTask(
  projectId: string,
  taskId: string,
  payload: { status: string; sortOrder: number; version: number },
) {
  const { data } = await api.patch<TaskItem>(`/projects/${projectId}/tasks/${taskId}/move`, payload)
  return data
}

export async function deleteTask(projectId: string, taskId: string) {
  await api.delete(`/projects/${projectId}/tasks/${taskId}`)
}

export async function getActivities(projectId: string) {
  const { data } = await api.get<ActivityItem[]>(`/projects/${projectId}/activities`)
  return data
}

export async function getAnalytics(projectId: string) {
  const { data } = await api.get<AnalyticsData>(`/projects/${projectId}/analytics`)
  return data
}
