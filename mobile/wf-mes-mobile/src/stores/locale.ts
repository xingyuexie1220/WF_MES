import { defineStore } from 'pinia'
import { ref } from 'vue'
import { localeOptions, setAppLocale, type AppLocale } from '@/i18n'
import { getLocale } from '@/utils/auth'

export const useLocaleStore = defineStore('wf-locale', () => {
  const locale = ref<AppLocale>((getLocale() as AppLocale) || 'zh-CN')

  function applyLocale(next: AppLocale) {
    locale.value = next
    setAppLocale(next)
  }

  function getLocaleLabel(value: AppLocale) {
    const item = localeOptions.find((option) => option.value === value)
    return item ? item.labelKey : 'locale.zhCN'
  }

  return { locale, localeOptions, applyLocale, getLocaleLabel }
})
