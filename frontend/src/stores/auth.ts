import { defineStore } from 'pinia'
import { getApiError } from '@/services/api'
import { getCurrentUser, login as loginRequest, register as registerRequest } from '@/services/taskflow'
import type { User } from '@/types'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: localStorage.getItem('taskflow_token') ?? '',
    user: null as User | null,
    loading: false,
    error: '',
  }),
  getters: {
    isAuthenticated: (state) => Boolean(state.token),
  },
  actions: {
    async login(email: string, password: string) {
      this.loading = true
      this.error = ''
      try {
        const response = await loginRequest(email, password)
        this.token = response.token
        this.user = response.user
        localStorage.setItem('taskflow_token', response.token)
      } catch (error) {
        this.error = 'Invalid email or password.'
        throw error
      } finally {
        this.loading = false
      }
    },
    async register(displayName: string, email: string, password: string) {
      this.loading = true
      this.error = ''
      try {
        const response = await registerRequest(displayName, email, password)
        this.token = response.token
        this.user = response.user
        localStorage.setItem('taskflow_token', response.token)
      } catch (error) {
        this.error = getApiError(error).message
        throw error
      } finally {
        this.loading = false
      }
    },
    async hydrate() {
      if (!this.token) return
      try {
        this.user = await getCurrentUser()
      } catch {
        this.logout()
      }
    },
    logout() {
      this.token = ''
      this.user = null
      localStorage.removeItem('taskflow_token')
    },
  },
})
