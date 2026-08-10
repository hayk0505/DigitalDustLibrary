import path from 'node:path'
import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'
import { tanstackRouter } from '@tanstack/router-plugin/vite'

export default defineConfig({
  plugins: [
    tailwindcss(),
    tanstackRouter({ target: 'react', autoCodeSplitting: true }),
    react(),
  ],
  resolve: {
    alias: { '@': path.resolve(__dirname, './src') },
  },
  server: {
    proxy: {
      // Proxies to apps/api (see ../../docker-compose.dev.yml, port 5080).
      // Keeps the browser's view of things same-origin in local dev too — the
      // httpOnly refresh cookie (Admin_Panel_Build_Spec.md's auth model) only
      // works reliably same-origin without extra CORS/SameSite gymnastics, and
      // this matches production, where the admin panel and API sit behind the
      // same Caddy host on the droplet. Only relevant once VITE_ENABLE_MOCKS is
      // switched off — MSW intercepts /api/* in the browser before requests
      // reach this proxy while mocks are on.
      '/api': {
        target: 'http://localhost:5080',
        changeOrigin: true,
      },
    },
  },
  test: {
    environment: 'jsdom',
    setupFiles: ['./src/test/setup.ts'],
    globals: false,
  },
})
