import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    port: 3000,
    proxy: {
      // All API calls proxied to the Gateway — no CORS issues in dev
      '/identity': 'http://localhost:5000',
      '/expenses': 'http://localhost:5000',
    },
  },
})
