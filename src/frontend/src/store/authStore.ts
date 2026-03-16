import { create } from 'zustand'
import { persist } from 'zustand/middleware'

/**
 * Global auth state — persisted to localStorage so the user
 * stays logged in across page refreshes.
 * Zustand keep this simple: no reducers, no boilerplate.
 */
interface AuthState {
  accessToken:  string | null
  refreshToken: string | null
  userId:       string | null
  setAuth: (accessToken: string, refreshToken: string, userId: string) => void
  clearAuth: () => void
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      accessToken:  null,
      refreshToken: null,
      userId:       null,
      setAuth: (accessToken, refreshToken, userId) =>
        set({ accessToken, refreshToken, userId }),
      clearAuth: () =>
        set({ accessToken: null, refreshToken: null, userId: null }),
    }),
    { name: 'expenseflow-auth' },
  ),
)
