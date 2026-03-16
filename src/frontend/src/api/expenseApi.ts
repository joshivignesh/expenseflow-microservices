import { apiClient } from './apiClient'

export type ExpenseStatus   = 'Draft' | 'Submitted' | 'Approved' | 'Rejected'
export type ExpenseCategory = 'Travel' | 'Accommodation' | 'Meals' | 'Office' | 'Software' | 'Training' | 'Marketing' | 'Other'

export interface Expense {
  id:                string
  submittedByUserId: string
  description:       string
  amount:            number
  currency:          string
  category:          number
  status:            number
  expenseDate:       string
  createdAt:         string
  submittedAt?:      string
  reviewedAt?:       string
  reviewedByUserId?: string
  rejectionReason?:  string
}

export interface CreateExpenseRequest {
  description: string
  amount:      number
  currency:    string
  category:    number
  expenseDate: string
}

export interface CreateExpenseResponse {
  expenseId: string
  status:    string
  createdAt: string
}

export const expenseApi = {
  create: (data: CreateExpenseRequest) =>
    apiClient.post<CreateExpenseResponse>('/expenses', data).then(r => r.data),

  getById: (id: string) =>
    apiClient.get<Expense>(`/expenses/${id}`).then(r => r.data),

  getMy: () =>
    apiClient.get<Expense[]>('/expenses/my').then(r => r.data),

  getPending: () =>
    apiClient.get<Expense[]>('/expenses/pending').then(r => r.data),

  submit: (id: string) =>
    apiClient.post(`/expenses/${id}/submit`),

  approve: (id: string) =>
    apiClient.post(`/expenses/${id}/approve`),

  reject: (id: string, reason: string) =>
    apiClient.post(`/expenses/${id}/reject`, { reason }),
}
