<script setup lang="ts">
import { onMounted, reactive } from 'vue'
import { useRouter } from 'vue-router'
import ProjectCard from '@/components/ProjectCard.vue'
import EmptyState from '@/components/EmptyState.vue'
import LoadingState from '@/components/LoadingState.vue'
import { useProjectStore } from '@/stores/projects'

const projects = useProjectStore()
const router = useRouter()

const form = reactive({
  name: '',
  description: '',
})

onMounted(() => {
  projects.fetchProjects()
})

async function createProject() {
  if (!form.name.trim()) return
  const project = await projects.addProject(form.name.trim(), form.description.trim())
  form.name = ''
  form.description = ''
  router.push(`/projects/${project.id}`)
}
</script>

<template>
  <section class="page-header">
    <div>
      <p class="eyebrow">Overview</p>
      <h1>Your projects</h1>
      <p class="muted">Track delivery across teams with a responsive workspace.</p>
    </div>
  </section>

  <section class="panel create-panel">
    <h2>Create project</h2>
    <form class="inline-form" @submit.prevent="createProject">
      <input v-model="form.name" placeholder="Project name" aria-label="Project name" required />
      <input v-model="form.description" placeholder="Short description" aria-label="Project description" />
      <button type="submit" class="primary-button">Create</button>
    </form>
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
</template>
