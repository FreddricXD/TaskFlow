<script setup lang="ts">
import { reactive, ref, toRef, watch } from 'vue'
import { useModalLifecycle } from '@/composables/useModalLifecycle'
import type { ProjectDetail } from '@/types'

const props = defineProps<{
  open: boolean
  project: ProjectDetail | null
  saving?: boolean
  error?: string
  canDelete?: boolean
}>()

const emit = defineEmits<{
  'update:open': [boolean]
  addMember: [{ email: string; role: 'Member' | 'Admin' }]
  deleteProject: []
}>()

const form = reactive({
  email: '',
  role: 'Member' as 'Member' | 'Admin',
})
const emailInput = ref<HTMLInputElement | null>(null)

function close() {
  if (props.saving) return
  emit('update:open', false)
}

function addMember() {
  const email = form.email.trim()
  if (!email) return
  emit('addMember', { email, role: form.role })
}

watch(
  () => props.project?.members.length,
  () => {
    form.email = ''
    form.role = 'Member'
  },
)

useModalLifecycle(toRef(props, 'open'), close, emailInput)
</script>

<template>
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="open" class="modal-backdrop" @click.self="close">
        <section class="modal" role="dialog" aria-modal="true" aria-labelledby="project-settings-title">
          <header class="modal-header">
            <div>
              <p class="eyebrow">Collaboration</p>
              <h2 id="project-settings-title">Project members</h2>
            </div>
            <button type="button" class="icon-button" aria-label="Close project settings" @click="close">×</button>
          </header>

          <div class="modal-body project-settings-body">
            <p v-if="error" class="error-banner modal-error" role="alert">{{ error }}</p>

            <section>
              <div class="settings-section-heading">
                <div>
                  <h3>Team access</h3>
                  <p>Add an existing TaskFlow account by email.</p>
                </div>
                <span class="metric-chip">{{ project?.members.length ?? 0 }} members</span>
              </div>

              <form class="member-form" @submit.prevent="addMember">
                <label>
                  Member email
                  <input ref="emailInput" v-model="form.email" type="email" placeholder="teammate@example.com" required />
                </label>
                <label>
                  Access level
                  <select v-model="form.role">
                    <option value="Member">Member</option>
                    <option value="Admin">Admin</option>
                  </select>
                </label>
                <button type="submit" class="primary-button" :disabled="saving">
                  {{ saving ? 'Adding…' : 'Add member' }}
                </button>
              </form>
            </section>

            <ul class="member-list" aria-label="Project members">
              <li v-for="member in project?.members" :key="member.id">
                <span class="member-avatar">{{ member.displayName.slice(0, 1).toUpperCase() }}</span>
                <div>
                  <strong>{{ member.displayName }}</strong>
                  <span>{{ member.email }}</span>
                </div>
                <span class="member-role">{{ member.role }}</span>
              </li>
            </ul>

            <section v-if="canDelete" class="danger-zone">
              <div>
                <h3>Delete project</h3>
                <p>Permanently removes this project, its tasks, and activity history.</p>
              </div>
              <button type="button" class="danger-button" :disabled="saving" @click="emit('deleteProject')">
                Delete project
              </button>
            </section>
          </div>
        </section>
      </div>
    </Transition>
  </Teleport>
</template>
