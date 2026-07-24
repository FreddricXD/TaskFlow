import axios, { type AxiosError } from 'axios'
import type { ApiError } from '@/types'

const api = axios.create({
  baseURL: '/api',
})

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('taskflow_token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

export function getApiError(error: unknown): ApiError {
  if (axios.isAxiosError(error)) {
    const axiosError = error as AxiosError<ApiError>
    return axiosError.response?.data ?? { message: axiosError.message }
  }

  return { message: 'Unexpected error occurred.' }
}

export default api
