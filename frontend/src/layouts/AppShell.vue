<script setup lang="ts">
import { computed, ref } from 'vue'
import { RouterLink, RouterView, useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()
const router = useRouter()
const route = useRoute()
const menuOpen = ref(false)

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
</script>

<template>
  <div class="shell">
    <header class="topbar">
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
        <span>{{ auth.user?.displayName }}</span>
        <button type="button" class="ghost-button" @click="logout">Sign out</button>
      </div>
    </header>

    <div class="shell-body" :class="{ 'menu-open': menuOpen }">
      <aside class="sidebar" @click="closeMenu">
        <nav>
          <RouterLink to="/" @click="closeMenu">Dashboard</RouterLink>
          <p class="sidebar-label">Collaboration</p>
          <RouterLink v-if="route.params.projectId" :to="`/projects/${route.params.projectId}`" @click="closeMenu">
            Active board
          </RouterLink>
        </nav>
      </aside>

      <main class="content">
        <RouterView />
      </main>
    </div>
  </div>
</template>
