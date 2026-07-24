import { defineStore } from 'pinia'

export type ThemeMode = 'light' | 'dark' | 'system'

const STORAGE_KEY = 'taskflow_theme'

function resolveTheme(mode: ThemeMode): 'light' | 'dark' {
  if (mode === 'system') {
    return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
  }

  return mode
}

function applyTheme(mode: ThemeMode) {
  document.documentElement.dataset.mode = mode
  document.documentElement.dataset.theme = resolveTheme(mode)
}

export const useThemeStore = defineStore('theme', {
  state: () => ({
    mode: (localStorage.getItem(STORAGE_KEY) as ThemeMode | null) ?? 'system',
  }),
  getters: {
    resolvedTheme: (state): 'light' | 'dark' => resolveTheme(state.mode),
  },
  actions: {
    init() {
      applyTheme(this.mode)

      window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', () => {
        if (this.mode === 'system') {
          applyTheme('system')
        }
      })
    },
    setMode(mode: ThemeMode) {
      this.mode = mode
      localStorage.setItem(STORAGE_KEY, mode)
      applyTheme(mode)
    },
  },
})

export function initTheme() {
  const stored = (localStorage.getItem(STORAGE_KEY) as ThemeMode | null) ?? 'system'
  applyTheme(stored)
}
