import { defineConfig, loadEnv } from 'vite';
import react from '@vitejs/plugin-react';
import path from 'path';

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => {
  // Load env variables passed from Docker or .env files
  const env = loadEnv(mode, process.cwd(), '');

  return {
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
    // Vite handles import.meta.env automatically for variables starting with VITE_
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
      },
      proxy: {
        '/api': {
          target: env.VITE_API_BASE_URL || 'http://gateway.api:8080',
          changeOrigin: true,
          rewrite: (path) => path.replace(/^\/api/, '')
        }
      }
    },
    plugins: [react()],
  };
});