import { useForm } from 'react-hook-form'
import { Link } from 'react-router-dom'
import { useLogin } from '../hooks/useAuth'
import { LoginRequest } from '../api/authApi'
import styles from './AuthPage.module.css'

export default function LoginPage() {
  const { register, handleSubmit, formState: { errors } } = useForm<LoginRequest>()
  const login = useLogin()

  return (
    <div className={styles.container}>
      <div className={styles.card}>
        <h1 className={styles.title}>ExpenseFlow</h1>
        <p className={styles.subtitle}>Sign in to your account</p>

        <form onSubmit={handleSubmit((data) => login.mutate(data))}>
          <div className={styles.field}>
            <label>Email</label>
            <input
              type="email"
              {...register('email', { required: 'Email is required' })}
              placeholder="you@company.com"
            />
            {errors.email && <span className={styles.error}>{errors.email.message}</span>}
          </div>

          <div className={styles.field}>
            <label>Password</label>
            <input
              type="password"
              {...register('password', { required: 'Password is required' })}
              placeholder="••••••••"
            />
            {errors.password && <span className={styles.error}>{errors.password.message}</span>}
          </div>

          {login.isError && (
            <p className={styles.apiError}>Invalid email or password.</p>
          )}

          <button
            type="submit"
            className={styles.btn}
            disabled={login.isPending}
          >
            {login.isPending ? 'Signing in…' : 'Sign In'}
          </button>
        </form>

        <p className={styles.footer}>
          Don't have an account? <Link to="/register">Register</Link>
        </p>
      </div>
    </div>
  )
}
