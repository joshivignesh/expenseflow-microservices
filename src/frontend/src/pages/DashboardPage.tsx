import { Link } from 'react-router-dom'
import { useMyExpenses } from '../hooks/useExpenses'
import { useLogout } from '../hooks/useAuth'
import { useAuthStore } from '../store/authStore'
import { Expense } from '../api/expenseApi'
import styles from './DashboardPage.module.css'

const STATUS_LABEL: Record<number, string> = {
  0: 'Draft', 1: 'Submitted', 2: 'Approved', 3: 'Rejected',
}
const STATUS_COLOR: Record<number, string> = {
  0: '#6b7280', 1: '#f59e0b', 2: '#10b981', 3: '#ef4444',
}
const CATEGORY_LABEL: Record<number, string> = {
  1: 'Travel', 2: 'Accommodation', 3: 'Meals',
  4: 'Office', 5: 'Software', 6: 'Training', 7: 'Marketing', 99: 'Other',
}

function ExpenseRow({ expense }: { expense: Expense }) {
  return (
    <Link to={`/expenses/${expense.id}`} className={styles.row}>
      <div className={styles.rowMain}>
        <span className={styles.desc}>{expense.description}</span>
        <span className={styles.category}>{CATEGORY_LABEL[expense.category] ?? 'Other'}</span>
      </div>
      <div className={styles.rowMeta}>
        <span className={styles.amount}>{expense.amount.toLocaleString()} {expense.currency}</span>
        <span className={styles.badge} style={{ background: STATUS_COLOR[expense.status] }}>
          {STATUS_LABEL[expense.status]}
        </span>
        <span className={styles.date}>
          {new Date(expense.expenseDate).toLocaleDateString()}
        </span>
      </div>
    </Link>
  )
}

export default function DashboardPage() {
  const { data: expenses, isLoading, isError } = useMyExpenses()
  const userId = useAuthStore((s) => s.userId)
  const logout = useLogout()

  return (
    <div className={styles.page}>
      <header className={styles.header}>
        <h1>ExpenseFlow</h1>
        <div className={styles.headerRight}>
          <span className={styles.uid}>ID: {userId?.slice(0, 8)}…</span>
          <button className={styles.logoutBtn} onClick={logout}>Logout</button>
        </div>
      </header>

      <main className={styles.main}>
        <div className={styles.toolbar}>
          <h2>My Expenses</h2>
          <Link to="/expenses/new" className={styles.newBtn}>+ New Expense</Link>
        </div>

        {isLoading && <p className={styles.state}>Loading expenses…</p>}
        {isError   && <p className={styles.stateErr}>Failed to load expenses.</p>}

        {expenses?.length === 0 && (
          <div className={styles.empty}>
            <p>No expenses yet.</p>
            <Link to="/expenses/new">Create your first expense →</Link>
          </div>
        )}

        <div className={styles.list}>
          {expenses?.map((e) => <ExpenseRow key={e.id} expense={e} />)}
        </div>
      </main>
    </div>
  )
}
