import { defineStore } from 'pinia'
import { getApiError } from '@/services/api'
import {
  addProjectMember,
  createProject,
  deleteProject,
  getProject,
  getProjects,
} from '@/services/taskflow'
import type { Project, ProjectDetail } from '@/types'

export const useProjectStore = defineStore('projects', {
  state: () => ({
    projects: [] as Project[],
    currentProject: null as ProjectDetail | null,
    loading: false,
    saving: false,
    error: '',
    settingsError: '',
  }),
  actions: {
    async fetchProjects() {
      this.loading = true
      this.error = ''
      try {
        this.projects = await getProjects()
      } catch {
        this.error = 'Unable to load projects.'
      } finally {
        this.loading = false
      }
    },
    async fetchProject(projectId: string) {
      this.loading = true
      this.error = ''
      try {
        this.currentProject = await getProject(projectId)
      } catch {
        this.error = 'Unable to load project.'
      } finally {
        this.loading = false
      }
    },
    async addProject(name: string, description: string) {
      const project = await createProject(name, description)
      await this.fetchProjects()
      return project
    },
    async addMember(projectId: string, email: string, role: 'Member' | 'Admin') {
      this.saving = true
      this.settingsError = ''
      try {
        const member = await addProjectMember(projectId, email, role)
        this.currentProject?.members.push(member)
        await this.fetchProjects()
        return member
      } catch (error) {
        this.settingsError = getApiError(error).message
        throw error
      } finally {
        this.saving = false
      }
    },
    async removeProject(projectId: string) {
      this.saving = true
      this.settingsError = ''
      try {
        await deleteProject(projectId)
        this.projects = this.projects.filter((project) => project.id !== projectId)
        this.currentProject = null
      } catch (error) {
        this.settingsError = getApiError(error).message
        throw error
      } finally {
        this.saving = false
      }
    },
  },
})
