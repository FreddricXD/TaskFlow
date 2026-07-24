<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import { RouterLink, useRouter } from 'vue-router'
import ThemeSwitcher from '@/components/ThemeSwitcher.vue'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()
const router = useRouter()
const showPassword = ref(false)
const submitted = ref(false)

const form = reactive({
  displayName: '',
  email: '',
  password: '',
  confirmPassword: '',
})

const passwordsMatch = computed(
  () => !form.confirmPassword || form.password === form.confirmPassword,
)
const passwordIsStrong = computed(
  () =>
    form.password.length >= 8 &&
    /[A-Z]/.test(form.password) &&
    /[a-z]/.test(form.password) &&
    /\d/.test(form.password),
)

async function submit() {
  submitted.value = true
  auth.error = ''
  if (!passwordsMatch.value || !passwordIsStrong.value) return

  try {
    await auth.register(form.displayName, form.email, form.password)
    router.push({ name: 'dashboard' })
  } catch {
    // API message is displayed by the auth store.
  }
}
</script>

<template>
  <div class="auth-page">
    <section class="auth-card">
      <div class="auth-card-top">
        <p class="eyebrow">TaskFlow</p>
        <ThemeSwitcher />
      </div>

      <h1>Create your account</h1>
      <p class="muted">Start a workspace and invite your team when you are ready.</p>

      <form @submit.prevent="submit">
        <label>
          Full name
          <input v-model="form.displayName" type="text" autocomplete="name" placeholder="Your name" required />
        </label>

        <label>
          Email
          <input v-model="form.email" type="email" autocomplete="email" placeholder="you@example.com" required />
        </label>

        <label>
          Password
          <div class="password-field">
            <input
              v-model="form.password"
              :type="showPassword ? 'text' : 'password'"
              autocomplete="new-password"
              placeholder="At least 8 characters"
              required
            />
            <button type="button" class="ghost-button" @click="showPassword = !showPassword">
              {{ showPassword ? 'Hide' : 'Show' }}
            </button>
          </div>
          <small class="field-hint">Use uppercase, lowercase, and a number.</small>
        </label>

        <label>
          Confirm password
          <input
            v-model="form.confirmPassword"
            type="password"
            autocomplete="new-password"
            placeholder="Repeat your password"
            required
          />
        </label>

        <p v-if="submitted && !passwordIsStrong" class="error-banner" role="alert">
          Password must be at least 8 characters with uppercase, lowercase, and a number.
        </p>
        <p v-else-if="submitted && !passwordsMatch" class="error-banner" role="alert">
          Passwords do not match.
        </p>
        <p v-else-if="auth.error" class="error-banner" role="alert">{{ auth.error }}</p>

        <button type="submit" class="primary-button auth-submit" :disabled="auth.loading">
          {{ auth.loading ? 'Creating account…' : 'Create account' }}
        </button>
      </form>

      <p class="auth-switch">
        Already have an account?
        <RouterLink to="/login">Sign in</RouterLink>
      </p>
    </section>

    <section class="auth-aside">
      <div class="auth-aside-content">
        <p class="eyebrow auth-eyebrow">A better way to deliver</p>
        <h2>Your projects, tasks, and team in one focused workspace.</h2>
        <ul>
          <li>Create projects and organize work visually</li>
          <li>Collaborate live with your team</li>
          <li>Track progress with actionable analytics</li>
        </ul>
      </div>
    </section>
  </div>
</template>
