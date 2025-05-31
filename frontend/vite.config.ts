import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path';

// https://vitejs.dev/config/
export default defineConfig({
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
      '@app': path.resolve(__dirname, './src/app'),
      '@entities': path.resolve(__dirname, './src/entities'),
      '@pages': path.resolve(__dirname, './src/pages'),
      '@shared': path.resolve(__dirname, './src/shared'),
      '@widgets': path.resolve(__dirname, './src/widgets')
    },
  },
  define: {
    'process.env': `"${process.env}"`
  },
  base: "/practices-service/",
  server: {
    allowedHosts: ['practices-service-spbu.ru', 'projects.se.math.spbu.ru'],
    host: true,
    port: 8000,
    hmr: {
        clientPort: 443,
        protocol: 'wss',
        host: 'projects.se.math.spbu.ru'
    },
    watch: {
      usePolling: true
    }
  },
  plugins: [react()],
})
