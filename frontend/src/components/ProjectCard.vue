<script setup lang="ts">
import { RouterLink } from 'vue-router'
import type { Project } from '@/types'

defineProps<{ project: Project }>()
</script>

<template>
  <article class="project-card">
    <div class="project-card-head">
      <h2>{{ project.name }}</h2>
      <span>{{ project.taskCount }} tasks</span>
    </div>
    <p>{{ project.description }}</p>
    <div class="project-card-meta">
      <span>{{ project.memberCount }} members</span>
      <span>Owner: {{ project.ownerName }}</span>
    </div>
    <RouterLink class="text-link" :to="`/projects/${project.id}`">Open board</RouterLink>
  </article>
</template>
