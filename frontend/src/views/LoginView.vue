<script setup lang="ts">
import { reactive, ref } from 'vue'
import { RouterLink, useRoute, useRouter } from 'vue-router'
import ThemeSwitcher from '@/components/ThemeSwitcher.vue'
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
      <div class="auth-card-top">
        <p class="eyebrow">TaskFlow</p>
        <ThemeSwitcher />
      </div>
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

      <p class="auth-switch">
        New to TaskFlow?
        <RouterLink to="/register">Create an account</RouterLink>
      </p>
    </section>

    <section class="auth-aside">
      <div class="auth-aside-content">
        <p class="eyebrow auth-eyebrow">Collaborative workspace</p>
        <h2>Plan, track, and ship together.</h2>
        <ul>
          <li>Responsive dashboard and Kanban board</li>
          <li>JWT-secured ASP.NET Core API</li>
          <li>Real-time updates and project analytics</li>
        </ul>
      </div>
    </section>
  </div>
</template>
