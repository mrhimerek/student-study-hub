import tailwindcss from '@tailwindcss/vite'
import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    proxy: {
      // Forwards /api/* to the local Azure Functions host (`func start`), so the browser
      // only ever talks to the same origin and we avoid CORS entirely in dev. In production
      // this is replaced by Azure Static Web Apps' built-in /api proxying to the Function App.
      '/api': {
        target: 'http://localhost:7071',
        changeOrigin: true,
      },
    },
  },
})
