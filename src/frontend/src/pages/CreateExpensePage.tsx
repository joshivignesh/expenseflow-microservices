import { useForm } from 'react-hook-form'
import { useNavigate, Link } from 'react-router-dom'
import { useCreateExpense } from '../hooks/useExpenses'
import { CreateExpenseRequest } from '../api/expenseApi'
import styles from './AuthPage.module.css'
import dashStyles from './DashboardPage.module.css'

const CATEGORIES = [
  { value: 1, label: 'Travel' },
  { value: 2, label: 'Accommodation' },
  { value: 3, label: 'Meals' },
  { value: 4, label: 'Office' },
  { value: 5, label: 'Software' },
  { value: 6, label: 'Training' },
  { value: 7, label: 'Marketing' },
  { value: 99, label: 'Other' },
]

export default function CreateExpensePage() {
  const { register, handleSubmit, formState: { errors } } = useForm<CreateExpenseRequest>()
  const createExpense = useCreateExpense()
  const navigate = useNavigate()

  const onSubmit = (data: CreateExpenseRequest) => {
    createExpense.mutate(
      { ...data, amount: Number(data.amount), category: Number(data.category) },
      { onSuccess: (res) => navigate(`/expenses/${res.expenseId}`) },
    )
  }

  return (
    <div style={{ minHeight: '100vh', background: '#f9fafb', padding: '2rem 1rem' }}>
      <div style={{ maxWidth: 520, margin: '0 auto' }}>
        <div style={{ marginBottom: '1.5rem' }}>
          <Link to="/" style={{ color: '#667eea', textDecoration: 'none', fontSize: '0.9rem' }}>
            ← Back to Dashboard
          </Link>
        </div>

        <div className={styles.card} style={{ maxWidth: '100%' }}>
          <h2 style={{ marginBottom: '1.5rem', fontSize: '1.25rem', fontWeight: 700 }}>
            New Expense
          </h2>

          <form onSubmit={handleSubmit(onSubmit)}>
            <div className={styles.field}>
              <label>Description</label>
              <input
                {...register('description', { required: 'Description is required' })}
                placeholder="e.g. Flight to Mumbai for client meeting"
              />
              {errors.description && <span className={styles.error}>{errors.description.message}</span>}
            </div>

            <div className={styles.row}>
              <div className={styles.field}>
                <label>Amount</label>
                <input
                  type="number"
                  step="0.01"
                  {...register('amount', { required: 'Required', min: { value: 0.01, message: 'Must be > 0' } })}
                  placeholder="0.00"
                />
                {errors.amount && <span className={styles.error}>{errors.amount.message}</span>}
              </div>
              <div className={styles.field}>
                <label>Currency</label>
                <input
                  {...register('currency', {
                    required: 'Required',
                    pattern: { value: /^[A-Za-z]{3}$/, message: '3-letter code (INR, USD)' },
                  })}
                  placeholder="INR"
                  maxLength={3}
                  style={{ textTransform: 'uppercase' }}
                />
                {errors.currency && <span className={styles.error}>{errors.currency.message}</span>}
              </div>
            </div>

            <div className={styles.field}>
              <label>Category</label>
              <select {...register('category', { required: 'Required' })}>
                <option value="">Select a category</option>
                {CATEGORIES.map((c) => (
                  <option key={c.value} value={c.value}>{c.label}</option>
                ))}
              </select>
              {errors.category && <span className={styles.error}>{errors.category.message}</span>}
            </div>

            <div className={styles.field}>
              <label>Expense Date</label>
              <input
                type="date"
                {...register('expenseDate', { required: 'Date is required' })}
                max={new Date().toISOString().split('T')[0]}
              />
              {errors.expenseDate && <span className={styles.error}>{errors.expenseDate.message}</span>}
            </div>

            {createExpense.isError && (
              <p className={styles.apiError}>Failed to create expense. Please try again.</p>
            )}

            <button type="submit" className={styles.btn} disabled={createExpense.isPending}>
              {createExpense.isPending ? 'Creating…' : 'Create Expense'}
            </button>
          </form>
        </div>
      </div>
    </div>
  )
}
