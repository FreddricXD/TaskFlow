<script setup lang="ts">
import { RouterLink } from 'vue-router'
import type { Project } from '@/types'

defineProps<{ project: Project }>()
</script>

<template>
  <article class="project-card">
    <div class="project-card-accent" />
    <div class="project-card-head">
      <h2>{{ project.name }}</h2>
      <span class="metric-chip">{{ project.taskCount }} tasks</span>
    </div>
    <p class="project-card-copy">{{ project.description }}</p>
    <div class="project-card-meta">
      <span>{{ project.memberCount }} members</span>
      <span>{{ project.ownerName }}</span>
    </div>
    <RouterLink class="primary-button project-card-action" :to="`/projects/${project.id}`">Open board</RouterLink>
  </article>
</template>
