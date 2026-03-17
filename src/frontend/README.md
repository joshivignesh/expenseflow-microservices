# ExpenseFlow Frontend

React SPA built with **Vite + TypeScript + TanStack Query**. Talks to the backend exclusively through the YARP Gateway — never directly to individual services.

## Tech Stack Decisions

| Library | Why chosen |
|---|---|
| **Vite** | 10x faster dev server than CRA; instant HMR; native ESM |
| **TanStack Query** | Purpose-built for server state; handles loading/error/stale automatically |
| **Zustand** | Auth state in 20 lines; no reducers; `persist` middleware for localStorage |
| **React Hook Form** | Zero re-renders on keystroke; built-in validation; tiny bundle |
| **React Router v6** | Declarative routing; `Navigate` for route guards |
| **Axios** | Interceptors for auto-attach JWT + global 401 handling |

## Architecture

```
src/
├── api/
│   ├── apiClient.ts       # Axios instance — JWT interceptor + 401 redirect
│   ├── authApi.ts         # register, login, getProfile
│   └── expenseApi.ts      # create, getById, getMy, getPending, submit, approve, reject
├── store/
│   └── authStore.ts       # Zustand — accessToken, refreshToken, userId (persisted)
├── hooks/
│   ├── useAuth.ts         # useLogin, useRegister, useLogout mutations
│   └── useExpenses.ts     # all expense queries and mutations with cache invalidation
└── pages/
    ├── LoginPage.tsx
    ├── RegisterPage.tsx
    ├── DashboardPage.tsx      # My Expenses list
    ├── CreateExpensePage.tsx  # New expense form
    └── ExpenseDetailPage.tsx  # Detail + Submit/Approve/Reject actions
```

## Key Internal Decisions

### Vite Dev Proxy
```ts
proxy: {
  '/identity': 'http://localhost:5000',
  '/expenses':  'http://localhost:5000',
}
```
Zero CORS issues in development. The same paths are proxied by Nginx in production Docker. API code never has environment-specific base URLs.

### TanStack Query Keys
```ts
export const expenseKeys = {
  my:     ['expenses', 'my'],
  detail: (id) => ['expenses', id],
  ...
}
```
Centralised keys mean mutations can invalidate exactly the right cached queries. After approving an expense, `qc.invalidateQueries({ queryKey: expenseKeys.pending })` triggers a background refetch of the approval queue — no manual state updates.

### JWT Auto-Attach
Axios request interceptor reads `useAuthStore.getState().accessToken` on every request. No need to pass tokens manually anywhere. On 401, the response interceptor clears auth and redirects to `/login` — handles token expiry silently.

### Route Guards
`PrivateRoute` checks `useAuthStore` for `accessToken`. If absent, `<Navigate to="/login" replace />` redirects before rendering the protected page. Zustand's `persist` middleware means the check works correctly across page refreshes.

## Production Deployment (Docker)

Two-stage Dockerfile:
1. `node:20-alpine` builds the Vite bundle (`npm run build`)
2. `nginx:alpine` serves `/dist` as static files

`nginx.conf` handles two concerns:
- **SPA fallback**: `try_files $uri $uri/ /index.html` — React Router owns all client-side routes
- **API proxy**: `/identity/` and `/expenses/` forwarded to `gateway:8080` via Docker internal DNS

## Local Development

```bash
cd src/frontend
npm install
npm run dev        # http://localhost:3000
```

Requires the backend stack running: `docker-compose up sqlserver identity-service expense-service gateway`
