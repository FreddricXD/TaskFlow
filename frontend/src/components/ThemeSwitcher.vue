<script setup lang="ts">
import { useThemeStore, type ThemeMode } from '@/stores/theme'

const theme = useThemeStore()

const options: { mode: ThemeMode; label: string; icon: string }[] = [
  { mode: 'light', label: 'Light', icon: '☀' },
  { mode: 'dark', label: 'Dark', icon: '☾' },
  { mode: 'system', label: 'Desktop', icon: '⌁' },
]
</script>

<template>
  <div class="theme-switcher" role="group" aria-label="Theme">
    <button
      v-for="option in options"
      :key="option.mode"
      type="button"
      class="theme-switcher__button"
      :class="{ active: theme.mode === option.mode }"
      :aria-pressed="theme.mode === option.mode"
      :title="`${option.label} theme`"
      @click="theme.setMode(option.mode)"
    >
      <span class="theme-switcher__icon" aria-hidden="true">{{ option.icon }}</span>
      <span class="theme-switcher__label">{{ option.label }}</span>
    </button>
  </div>
</template>
