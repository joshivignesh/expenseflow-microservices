import { apiClient } from './apiClient'

export interface RegisterRequest {
  firstName: string
  lastName:  string
  email:     string
  password:  string
}

export interface LoginRequest {
  email:    string
  password: string
}

export interface AuthResponse {
  userId:       string
  accessToken:  string
  refreshToken: string
  expiresAt:    string
}

export interface UserProfile {
  userId:    string
  firstName: string
  lastName:  string
  email:     string
  role:      string
}

export const authApi = {
  register: (data: RegisterRequest) =>
    apiClient.post<AuthResponse>('/identity/auth/register', data).then(r => r.data),

  login: (data: LoginRequest) =>
    apiClient.post<AuthResponse>('/identity/auth/login', data).then(r => r.data),

  getProfile: (userId: string) =>
    apiClient.get<UserProfile>(`/identity/auth/profile/${userId}`).then(r => r.data),
}
