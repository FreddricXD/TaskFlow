<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()
const router = useRouter()
const route = useRoute()

const form = reactive({
  email: 'alice@taskflow.dev',
  password: 'Password123!',
})

const showPassword = ref(false)

async function submit() {
  try {
    await auth.login(form.email, form.password)
    const redirect = typeof route.query.redirect === 'string' ? route.query.redirect : '/'
    router.push(redirect)
  } catch {
    // Error handled in store
  }
}
</script>

<template>
  <div class="auth-page">
    <section class="auth-card">
      <p class="eyebrow">TaskFlow</p>
      <h1>Sign in to your workspace</h1>
      <p class="muted">Demo account: alice@taskflow.dev / Password123!</p>

      <form @submit.prevent="submit">
        <label>
          Email
          <input v-model="form.email" type="email" autocomplete="username" required />
        </label>

        <label>
          Password
          <div class="password-field">
            <input
              v-model="form.password"
              :type="showPassword ? 'text' : 'password'"
              autocomplete="current-password"
              required
            />
            <button type="button" class="ghost-button" @click="showPassword = !showPassword">
              {{ showPassword ? 'Hide' : 'Show' }}
            </button>
          </div>
        </label>

        <p v-if="auth.error" class="error-banner" role="alert">{{ auth.error }}</p>

        <button type="submit" class="primary-button" :disabled="auth.loading">
          {{ auth.loading ? 'Signing in…' : 'Sign in' }}
        </button>
      </form>
    </section>

    <section class="auth-aside">
      <h2>Built for interview-ready collaboration</h2>
      <ul>
        <li>Responsive dashboard and Kanban board</li>
        <li>JWT-secured ASP.NET Core API</li>
        <li>Real-time updates and analytics-ready architecture</li>
      </ul>
    </section>
  </div>
</template>
