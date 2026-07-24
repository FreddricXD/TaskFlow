<script setup lang="ts">
import { reactive, ref, toRef, watch } from 'vue'
import { useModalLifecycle } from '@/composables/useModalLifecycle'

const props = defineProps<{
  open: boolean
  saving?: boolean
}>()

const emit = defineEmits<{
  'update:open': [boolean]
  save: [{ name: string; description: string }]
}>()

const form = reactive({
  name: '',
  description: '',
})
const nameInput = ref<HTMLInputElement | null>(null)

function close() {
  if (props.saving) return
  emit('update:open', false)
}

function submit() {
  const name = form.name.trim()
  if (!name) return
  emit('save', { name, description: form.description.trim() })
}

watch(
  () => props.open,
  (open) => {
    if (open) {
      form.name = ''
      form.description = ''
    }
  },
)

useModalLifecycle(toRef(props, 'open'), close, nameInput)
</script>

<template>
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="open" class="modal-backdrop" @click.self="close">
        <section class="modal modal--compact" role="dialog" aria-modal="true" aria-labelledby="project-modal-title">
          <header class="modal-header">
            <div>
              <p class="eyebrow">New workspace</p>
              <h2 id="project-modal-title">Create a project</h2>
            </div>
            <button type="button" class="icon-button" aria-label="Close project dialog" @click="close">×</button>
          </header>

          <form class="modal-form" @submit.prevent="submit">
            <div class="modal-body">
              <label>
                Project name
                <input ref="nameInput" v-model="form.name" placeholder="e.g. Product launch" required />
              </label>

              <label>
                Description
                <textarea
                  v-model="form.description"
                  rows="4"
                  placeholder="What is this project aiming to achieve?"
                />
              </label>
            </div>

            <footer class="modal-footer">
              <button type="button" class="ghost-button" :disabled="saving" @click="close">Cancel</button>
              <button type="submit" class="primary-button" :disabled="saving || !form.name.trim()">
                {{ saving ? 'Creating…' : 'Create project' }}
              </button>
            </footer>
          </form>
        </section>
      </div>
    </Transition>
  </Teleport>
</template>
