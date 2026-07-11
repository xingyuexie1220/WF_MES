import { ref } from 'vue'
import { defineStore } from 'pinia'

export type AppTheme = 'blue'

export const useAppStore = defineStore('wf-app', () => {
  const globalLoading = ref(false)
  const theme = ref<AppTheme>('blue')

  function setGlobalLoading(value: boolean) {
    globalLoading.value = value
  }

  function setTheme(value: AppTheme) {
    theme.value = value
  }

  return {
    globalLoading,
    theme,
    setGlobalLoading,
    setTheme
  }
})
