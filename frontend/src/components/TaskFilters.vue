<script setup lang="ts">
import { useTaskStore } from '@/stores/tasks'

const tasks = useTaskStore()
</script>

<template>
  <section class="panel filters-panel">
    <label>
      Search
      <input
        v-model="tasks.filters.search"
        type="search"
        placeholder="Search tasks"
        aria-label="Search tasks"
      />
    </label>

    <label>
      Status
      <select v-model="tasks.filters.status" aria-label="Filter by status">
        <option value="">All statuses</option>
        <option value="Todo">To Do</option>
        <option value="InProgress">In Progress</option>
        <option value="Review">Review</option>
        <option value="Done">Done</option>
      </select>
    </label>

    <label>
      Priority
      <select v-model="tasks.filters.priority" aria-label="Filter by priority">
        <option value="">All priorities</option>
        <option value="Low">Low</option>
        <option value="Medium">Medium</option>
        <option value="High">High</option>
        <option value="Critical">Critical</option>
      </select>
    </label>
  </section>
</template>
