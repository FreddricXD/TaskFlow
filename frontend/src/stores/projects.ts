import { defineStore } from 'pinia'
import { createProject, getProject, getProjects } from '@/services/taskflow'
import type { Project, ProjectDetail } from '@/types'

export const useProjectStore = defineStore('projects', {
  state: () => ({
    projects: [] as Project[],
    currentProject: null as ProjectDetail | null,
    loading: false,
    error: '',
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
  },
})
