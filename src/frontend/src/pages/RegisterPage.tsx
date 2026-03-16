import { useForm } from 'react-hook-form'
import { Link } from 'react-router-dom'
import { useRegister } from '../hooks/useAuth'
import { RegisterRequest } from '../api/authApi'
import styles from './AuthPage.module.css'

export default function RegisterPage() {
  const { register, handleSubmit, formState: { errors } } = useForm<RegisterRequest>()
  const registerMutation = useRegister()

  return (
    <div className={styles.container}>
      <div className={styles.card}>
        <h1 className={styles.title}>ExpenseFlow</h1>
        <p className={styles.subtitle}>Create your account</p>

        <form onSubmit={handleSubmit((data) => registerMutation.mutate(data))}>
          <div className={styles.row}>
            <div className={styles.field}>
              <label>First Name</label>
              <input {...register('firstName', { required: 'Required' })} placeholder="Vignesh" />
              {errors.firstName && <span className={styles.error}>{errors.firstName.message}</span>}
            </div>
            <div className={styles.field}>
              <label>Last Name</label>
              <input {...register('lastName', { required: 'Required' })} placeholder="Joshi" />
              {errors.lastName && <span className={styles.error}>{errors.lastName.message}</span>}
            </div>
          </div>

          <div className={styles.field}>
            <label>Email</label>
            <input type="email" {...register('email', { required: 'Required' })} placeholder="you@company.com" />
            {errors.email && <span className={styles.error}>{errors.email.message}</span>}
          </div>

          <div className={styles.field}>
            <label>Password</label>
            <input
              type="password"
              {...register('password', {
                required: 'Required',
                minLength: { value: 8, message: 'Minimum 8 characters' },
              })}
              placeholder="Min 8 characters"
            />
            {errors.password && <span className={styles.error}>{errors.password.message}</span>}
          </div>

          {registerMutation.isError && (
            <p className={styles.apiError}>Registration failed. Email may already be taken.</p>
          )}

          <button type="submit" className={styles.btn} disabled={registerMutation.isPending}>
            {registerMutation.isPending ? 'Creating account…' : 'Create Account'}
          </button>
        </form>

        <p className={styles.footer}>
          Already have an account? <Link to="/login">Sign in</Link>
        </p>
      </div>
    </div>
  )
}
