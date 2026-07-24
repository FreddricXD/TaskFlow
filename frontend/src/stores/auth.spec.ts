import { createPinia, setActivePinia } from 'pinia'
import { beforeEach, describe, expect, it } from 'vitest'
import { useAuthStore } from '@/stores/auth'

describe('auth store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    localStorage.clear()
  })

  it('starts unauthenticated without a token', () => {
    const auth = useAuthStore()
    expect(auth.isAuthenticated).toBe(false)
    expect(auth.user).toBeNull()
  })

  it('clears persisted auth on logout', () => {
    localStorage.setItem('taskflow_token', 'demo-token')
    const auth = useAuthStore()
    auth.token = 'demo-token'

    auth.logout()

    expect(auth.isAuthenticated).toBe(false)
    expect(localStorage.getItem('taskflow_token')).toBeNull()
  })
})
