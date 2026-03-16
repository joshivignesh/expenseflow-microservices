import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { expenseApi, CreateExpenseRequest } from '../api/expenseApi'

// Query keys — centralised so invalidation is consistent
export const expenseKeys = {
  all:     ['expenses'] as const,
  my:      ['expenses', 'my'] as const,
  pending: ['expenses', 'pending'] as const,
  detail:  (id: string) => ['expenses', id] as const,
}

/** Fetches the current user's expenses — "My Expenses" dashboard list */
export function useMyExpenses() {
  return useQuery({
    queryKey: expenseKeys.my,
    queryFn:  expenseApi.getMy,
  })
}

/** Fetches all submitted expenses — manager approval queue */
export function usePendingExpenses() {
  return useQuery({
    queryKey: expenseKeys.pending,
    queryFn:  expenseApi.getPending,
  })
}

/** Fetches a single expense by ID */
export function useExpense(id: string) {
  return useQuery({
    queryKey: expenseKeys.detail(id),
    queryFn:  () => expenseApi.getById(id),
    enabled:  !!id,
  })
}

/** Creates a new expense — invalidates the my-expenses list on success */
export function useCreateExpense() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (data: CreateExpenseRequest) => expenseApi.create(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: expenseKeys.my }),
  })
}

/** Submits a Draft expense — invalidates detail + my list */
export function useSubmitExpense(id: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: () => expenseApi.submit(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: expenseKeys.detail(id) })
      qc.invalidateQueries({ queryKey: expenseKeys.my })
    },
  })
}

/** Approves a Submitted expense — invalidates detail + pending list */
export function useApproveExpense(id: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: () => expenseApi.approve(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: expenseKeys.detail(id) })
      qc.invalidateQueries({ queryKey: expenseKeys.pending })
    },
  })
}

/** Rejects a Submitted expense with a reason */
export function useRejectExpense(id: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (reason: string) => expenseApi.reject(id, reason),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: expenseKeys.detail(id) })
      qc.invalidateQueries({ queryKey: expenseKeys.pending })
    },
  })
}
