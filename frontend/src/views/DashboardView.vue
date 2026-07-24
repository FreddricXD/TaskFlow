<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import ProjectCard from '@/components/ProjectCard.vue'
import EmptyState from '@/components/EmptyState.vue'
import LoadingState from '@/components/LoadingState.vue'
import ProjectModal from '@/components/ProjectModal.vue'
import { useProjectStore } from '@/stores/projects'

const projects = useProjectStore()
const router = useRouter()

const createOpen = ref(false)
const creating = ref(false)

const totalTasks = computed(() => projects.projects.reduce((total, project) => total + project.taskCount, 0))
const totalMembers = computed(() => projects.projects.reduce((total, project) => total + project.memberCount, 0))

onMounted(() => {
  projects.fetchProjects()
})

async function createProject(payload: { name: string; description: string }) {
  creating.value = true
  try {
    const project = await projects.addProject(payload.name, payload.description)
    createOpen.value = false
    router.push(`/projects/${project.id}`)
  } finally {
    creating.value = false
  }
}
</script>

<template>
  <section class="page-header">
    <div>
      <p class="eyebrow">Overview</p>
      <h1>Your projects</h1>
      <p class="muted">Track delivery across teams with a responsive workspace.</p>
    </div>
    <button type="button" class="primary-button page-action" @click="createOpen = true">
      <span aria-hidden="true">＋</span>
      New project
    </button>
  </section>

  <section class="summary-grid" aria-label="Workspace summary">
    <article class="summary-card">
      <span class="summary-card__icon" aria-hidden="true">▦</span>
      <div>
        <strong>{{ projects.projects.length }}</strong>
        <span>Active projects</span>
      </div>
    </article>
    <article class="summary-card">
      <span class="summary-card__icon" aria-hidden="true">✓</span>
      <div>
        <strong>{{ totalTasks }}</strong>
        <span>Total tasks</span>
      </div>
    </article>
    <article class="summary-card">
      <span class="summary-card__icon" aria-hidden="true">◎</span>
      <div>
        <strong>{{ totalMembers }}</strong>
        <span>Project members</span>
      </div>
    </article>
  </section>

  <LoadingState v-if="projects.loading" label="Loading projects" />

  <p v-else-if="projects.error" class="error-banner" role="alert">{{ projects.error }}</p>

  <EmptyState
    v-else-if="!projects.projects.length"
    title="No projects yet"
    description="Create your first project to open the Kanban board."
  />

  <div v-else class="project-grid">
    <ProjectCard v-for="project in projects.projects" :key="project.id" :project="project" />
  </div>

  <ProjectModal v-model:open="createOpen" :saving="creating" @save="createProject" />
</template>
