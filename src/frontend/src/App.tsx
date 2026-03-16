import { Routes, Route, Navigate } from 'react-router-dom'
import { useAuthStore } from './store/authStore'
import LoginPage from './pages/LoginPage'
import RegisterPage from './pages/RegisterPage'
import DashboardPage from './pages/DashboardPage'
import CreateExpensePage from './pages/CreateExpensePage'
import ExpenseDetailPage from './pages/ExpenseDetailPage'

/** Route guard — redirects unauthenticated users to /login */
function PrivateRoute({ children }: { children: React.ReactNode }) {
  const token = useAuthStore((s) => s.accessToken)
  return token ? <>{children}</> : <Navigate to="/login" replace />
}

export default function App() {
  return (
    <Routes>
      <Route path="/login"    element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />

      <Route path="/" element={
        <PrivateRoute><DashboardPage /></PrivateRoute>
      } />
      <Route path="/expenses/new" element={
        <PrivateRoute><CreateExpensePage /></PrivateRoute>
      } />
      <Route path="/expenses/:id" element={
        <PrivateRoute><ExpenseDetailPage /></PrivateRoute>
      } />

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}
