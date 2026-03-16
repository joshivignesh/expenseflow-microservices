import { useState } from 'react'
import { useParams, Link } from 'react-router-dom'
import { useExpense, useSubmitExpense, useApproveExpense, useRejectExpense } from '../hooks/useExpenses'
import { useAuthStore } from '../store/authStore'
import styles from './DashboardPage.module.css'

const STATUS_LABEL: Record<number, string> = {
  0: 'Draft', 1: 'Submitted', 2: 'Approved', 3: 'Rejected',
}
const STATUS_COLOR: Record<number, string> = {
  0: '#6b7280', 1: '#f59e0b', 2: '#10b981', 3: '#ef4444',
}

export default function ExpenseDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { data: expense, isLoading, isError } = useExpense(id!)
  const userId  = useAuthStore((s) => s.userId)

  const submit  = useSubmitExpense(id!)
  const approve = useApproveExpense(id!)
  const reject  = useRejectExpense(id!)

  const [rejectReason, setRejectReason] = useState('')
  const [showReject, setShowReject] = useState(false)

  if (isLoading) return <p style={{ padding: '2rem', textAlign: 'center' }}>Loading…</p>
  if (isError || !expense) return <p style={{ padding: '2rem', textAlign: 'center', color: '#ef4444' }}>Expense not found.</p>

  const isOwner  = expense.submittedByUserId === userId
  const isDraft  = expense.status === 0
  const isSubmitted = expense.status === 1

  return (
    <div style={{ minHeight: '100vh', background: '#f9fafb', padding: '2rem 1rem' }}>
      <div style={{ maxWidth: 600, margin: '0 auto' }}>
        <div style={{ marginBottom: '1.5rem' }}>
          <Link to="/" style={{ color: '#667eea', textDecoration: 'none', fontSize: '0.9rem' }}>
            ← Back to Dashboard
          </Link>
        </div>

        <div style={{ background: '#fff', borderRadius: 12, padding: '2rem', border: '1px solid #e5e7eb' }}>
          {/* Header */}
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '1.5rem' }}>
            <h2 style={{ fontSize: '1.25rem', fontWeight: 700 }}>{expense.description}</h2>
            <span className={styles.badge} style={{ background: STATUS_COLOR[expense.status] }}>
              {STATUS_LABEL[expense.status]}
            </span>
          </div>

          {/* Details grid */}
          <dl style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem 2rem', marginBottom: '2rem' }}>
            {([
              ['Amount',       `${expense.amount.toLocaleString()} ${expense.currency}`],
              ['Expense Date', new Date(expense.expenseDate).toLocaleDateString()],
              ['Created',      new Date(expense.createdAt).toLocaleDateString()],
              ['Submitted',    expense.submittedAt ? new Date(expense.submittedAt).toLocaleDateString() : '—'],
              ['Reviewed',     expense.reviewedAt  ? new Date(expense.reviewedAt).toLocaleDateString()  : '—'],
            ] as [string, string][]).map(([label, value]) => (
              <div key={label}>
                <dt style={{ fontSize: '0.8rem', color: '#9ca3af', fontWeight: 600, textTransform: 'uppercase', letterSpacing: '0.05em' }}>{label}</dt>
                <dd style={{ fontWeight: 600, marginTop: '0.25rem' }}>{value}</dd>
              </div>
            ))}
          </dl>

          {expense.rejectionReason && (
            <div style={{ background: '#fef2f2', border: '1px solid #fecaca', borderRadius: 8, padding: '1rem', marginBottom: '1.5rem' }}>
              <p style={{ fontSize: '0.875rem', fontWeight: 600, color: '#b91c1c' }}>Rejection Reason</p>
              <p style={{ color: '#7f1d1d', marginTop: '0.25rem' }}>{expense.rejectionReason}</p>
            </div>
          )}

          {/* Actions */}
          <div style={{ display: 'flex', gap: '0.75rem', flexWrap: 'wrap' }}>
            {/* Employee: submit their own draft */}
            {isOwner && isDraft && (
              <button
                onClick={() => submit.mutate()}
                disabled={submit.isPending}
                style={{ padding: '0.6rem 1.5rem', background: '#667eea', color: '#fff',
                         border: 'none', borderRadius: 8, fontWeight: 600, cursor: 'pointer' }}
              >
                {submit.isPending ? 'Submitting…' : 'Submit for Approval'}
              </button>
            )}

            {/* Manager: approve */}
            {isSubmitted && (
              <button
                onClick={() => approve.mutate()}
                disabled={approve.isPending}
                style={{ padding: '0.6rem 1.5rem', background: '#10b981', color: '#fff',
                         border: 'none', borderRadius: 8, fontWeight: 600, cursor: 'pointer' }}
              >
                {approve.isPending ? 'Approving…' : 'Approve'}
              </button>
            )}

            {/* Manager: reject */}
            {isSubmitted && !showReject && (
              <button
                onClick={() => setShowReject(true)}
                style={{ padding: '0.6rem 1.5rem', background: '#fff', color: '#ef4444',
                         border: '1.5px solid #ef4444', borderRadius: 8, fontWeight: 600, cursor: 'pointer' }}
              >
                Reject
              </button>
            )}
          </div>

          {/* Rejection reason form */}
          {showReject && (
            <div style={{ marginTop: '1rem', display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
              <textarea
                value={rejectReason}
                onChange={(e) => setRejectReason(e.target.value)}
                placeholder="Reason for rejection (required)"
                rows={3}
                style={{ padding: '0.75rem', border: '1.5px solid #d1d5db', borderRadius: 8,
                         fontSize: '0.95rem', resize: 'vertical', outline: 'none' }}
              />
              <div style={{ display: 'flex', gap: '0.75rem' }}>
                <button
                  onClick={() => { if (rejectReason.trim()) reject.mutate(rejectReason) }}
                  disabled={reject.isPending || !rejectReason.trim()}
                  style={{ padding: '0.6rem 1.5rem', background: '#ef4444', color: '#fff',
                           border: 'none', borderRadius: 8, fontWeight: 600, cursor: 'pointer' }}
                >
                  {reject.isPending ? 'Rejecting…' : 'Confirm Rejection'}
                </button>
                <button
                  onClick={() => { setShowReject(false); setRejectReason('') }}
                  style={{ padding: '0.6rem 1.5rem', background: '#fff', color: '#6b7280',
                           border: '1.5px solid #d1d5db', borderRadius: 8, fontWeight: 600, cursor: 'pointer' }}
                >
                  Cancel
                </button>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
