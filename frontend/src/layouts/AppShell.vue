<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { RouterLink, RouterView, useRoute, useRouter } from 'vue-router'
import ThemeSwitcher from '@/components/ThemeSwitcher.vue'
import { useAuthStore } from '@/stores/auth'
import { useProjectStore } from '@/stores/projects'

const auth = useAuthStore()
const projects = useProjectStore()
const router = useRouter()
const route = useRoute()
const menuOpen = ref(false)
const shell = ref<HTMLElement | null>(null)
const topbar = ref<HTMLElement | null>(null)
let topbarObserver: ResizeObserver | null = null

const pageTitle = computed(() => {
  if (route.name === 'project') return 'Project Board'
  return 'Dashboard'
})

function logout() {
  auth.logout()
  router.push({ name: 'login' })
}

function closeMenu() {
  menuOpen.value = false
}

onMounted(() => {
  if (!projects.projects.length) {
    projects.fetchProjects()
  }

  topbarObserver = new ResizeObserver(([entry]) => {
    if (entry && shell.value) {
      shell.value.style.setProperty('--topbar-height', `${entry.borderBoxSize[0]?.blockSize ?? entry.contentRect.height}px`)
    }
  })

  if (topbar.value) {
    topbarObserver.observe(topbar.value)
  }
})

onUnmounted(() => {
  topbarObserver?.disconnect()
})
</script>

<template>
  <div ref="shell" class="shell">
    <header ref="topbar" class="topbar">
      <button class="menu-toggle" type="button" aria-label="Toggle navigation" @click="menuOpen = !menuOpen">
        <span />
        <span />
        <span />
      </button>

      <div class="brand">
        <RouterLink to="/" @click="closeMenu">TaskFlow</RouterLink>
        <p>{{ pageTitle }}</p>
      </div>

      <div class="user-panel">
        <ThemeSwitcher />
        <span class="user-name">{{ auth.user?.displayName }}</span>
        <button type="button" class="ghost-button sign-out-button" @click="logout">
          <span class="sign-out-button__icon" aria-hidden="true">↪</span>
          <span class="sign-out-button__label">Sign out</span>
        </button>
      </div>
    </header>

    <div class="shell-body" :class="{ 'menu-open': menuOpen }">
      <button
        v-if="menuOpen"
        type="button"
        class="sidebar-scrim"
        aria-label="Close navigation"
        @click="closeMenu"
      />

      <aside class="sidebar">
        <nav>
          <RouterLink to="/" @click="closeMenu">Dashboard</RouterLink>
          <p class="sidebar-label">Projects</p>
          <RouterLink
            v-for="project in projects.projects"
            :key="project.id"
            :to="`/projects/${project.id}`"
            class="sidebar-project"
            @click="closeMenu"
          >
            <span class="sidebar-project__marker" aria-hidden="true" />
            <span class="sidebar-project__name">{{ project.name }}</span>
            <span class="sidebar-project__count">{{ project.taskCount }}</span>
          </RouterLink>
          <p v-if="!projects.loading && !projects.projects.length" class="sidebar-empty">No projects yet</p>
        </nav>
      </aside>

      <main class="content">
        <RouterView />
      </main>
    </div>
  </div>
</template>
