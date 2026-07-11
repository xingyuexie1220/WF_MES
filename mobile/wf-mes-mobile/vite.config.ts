import { defineConfig, loadEnv } from 'vite'
import uni from '@dcloudio/vite-plugin-uni'

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, `${process.cwd()}/env`, 'VITE_')
  return {
    plugins: [uni()],
    envDir: 'env',
    css: {
      preprocessorOptions: {
        scss: {
          silenceDeprecations: ['legacy-js-api', 'color-functions', 'import']
        }
      }
    },
    define: {
      'import.meta.env.VITE_API_BASE_URL': JSON.stringify(env.VITE_API_BASE_URL),
      'import.meta.env.VITE_FACTORY_CODE': JSON.stringify(env.VITE_FACTORY_CODE),
      'import.meta.env.VITE_FACTORY_NAME': JSON.stringify(env.VITE_FACTORY_NAME),
      'import.meta.env.VITE_APP_NAME': JSON.stringify(env.VITE_APP_NAME)
    }
  }
})
