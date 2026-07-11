import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { fileURLToPath, URL } from 'node:url'

export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    }
  },
  build: {
    rollupOptions: {
      output: {
        manualChunks(id) {
          if (id.includes('node_modules/element-plus')) {
            return 'element-plus'
          }
          if (id.includes('node_modules/echarts')) {
            return 'echarts'
          }
          if (id.includes('node_modules/vxe-table') || id.includes('node_modules/vxe-pc-ui')) {
            return 'vxe-table'
          }
          if (id.includes('node_modules/xlsx')) {
            return 'xlsx'
          }
          if (
            id.includes('node_modules/vue/') ||
            id.includes('node_modules/vue-router') ||
            id.includes('node_modules/pinia') ||
            id.includes('node_modules/axios')
          ) {
            return 'vendor'
          }
        }
      }
    },
    chunkSizeWarningLimit: 800
  },
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5088',
        changeOrigin: true
      },
      '/hubs': {
        target: 'http://localhost:5088',
        changeOrigin: true,
        ws: true
      }
    }
  }
})
