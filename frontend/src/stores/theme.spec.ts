import { createPinia, setActivePinia } from 'pinia'
import { beforeEach, describe, expect, it } from 'vitest'
import { useThemeStore } from '@/stores/theme'

describe('theme store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    localStorage.clear()
    document.documentElement.removeAttribute('data-theme')
    document.documentElement.removeAttribute('data-mode')
  })

  it('defaults to desktop/system mode', () => {
    const theme = useThemeStore()
    expect(theme.mode).toBe('system')
  })

  it('applies light theme when selected', () => {
    const theme = useThemeStore()
    theme.setMode('light')
    expect(document.documentElement.dataset.theme).toBe('light')
    expect(document.documentElement.dataset.mode).toBe('light')
  })
})
