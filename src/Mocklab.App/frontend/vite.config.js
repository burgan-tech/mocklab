import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  base: '/_admin/',  // Important: Set base path for embedded mode
  build: {
    outDir: '../wwwroot/_mocklab',  // Build to wwwroot for embedding
    emptyOutDir: true,
  },
  css: {
    preprocessorOptions: {
      scss: {
        silenceDeprecations: ['import'],
      }
    }
  },
  server: {
    port: 3000,
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        secure: false,
      },
      '/_admin/mocks': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        secure: false,
      },
      '/_admin/logs': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        secure: false,
      },
      '/_admin/collections': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        secure: false,
      }
    }
  }
});
