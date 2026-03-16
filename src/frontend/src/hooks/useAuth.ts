import { useMutation } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { authApi, LoginRequest, RegisterRequest } from '../api/authApi'
import { useAuthStore } from '../store/authStore'

/**
 * Login mutation — Sonnet (brain 🧠) orchestrates the post-login flow:
 * store tokens, redirect to dashboard.
 */
export function useLogin() {
  const setAuth   = useAuthStore((s) => s.setAuth)
  const navigate  = useNavigate()

  return useMutation({
    mutationFn: (data: LoginRequest) => authApi.login(data),
    onSuccess: (res) => {
      setAuth(res.accessToken, res.refreshToken, res.userId)
      navigate('/')
    },
  })
}

/**
 * Register mutation — same flow as login on success.
 */
export function useRegister() {
  const setAuth  = useAuthStore((s) => s.setAuth)
  const navigate = useNavigate()

  return useMutation({
    mutationFn: (data: RegisterRequest) => authApi.register(data),
    onSuccess: (res) => {
      setAuth(res.accessToken, res.refreshToken, res.userId)
      navigate('/')
    },
  })
}

export function useLogout() {
  const clearAuth = useAuthStore((s) => s.clearAuth)
  const navigate  = useNavigate()

  return () => {
    clearAuth()
    navigate('/login')
  }
}
