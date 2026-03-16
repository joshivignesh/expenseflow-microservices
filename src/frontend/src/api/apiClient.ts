import axios from 'axios'
import { useAuthStore } from '../store/authStore'

/**
 * Central Axios instance.
 * Base URL is empty — Vite dev proxy routes /identity and /expenses
 * to the Gateway at localhost:5000.
 * In production (Docker), the frontend is served by Nginx which
 * proxies the same paths to the Gateway container.
 */
export const apiClient = axios.create({
  baseURL: '',
  headers: { 'Content-Type': 'application/json' },
})

// Attach JWT to every request automatically
apiClient.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

// Clear auth on 401 — token expired or invalid
apiClient.interceptors.response.use(
  (res) => res,
  (err) => {
    if (err.response?.status === 401) {
      useAuthStore.getState().clearAuth()
      window.location.href = '/login'
    }
    return Promise.reject(err)
  },
)
