export type AppLocale = 'zh-CN' | 'zh-TW' | 'en'

export const LOCALE_STORAGE_KEY = 'wf_locale'

export const localeOptions: Array<{ labelKey: string; value: AppLocale }> = [
  { labelKey: 'locale.zhCN', value: 'zh-CN' },
  { labelKey: 'locale.zhTW', value: 'zh-TW' },
  { labelKey: 'locale.en', value: 'en' }
]

export function getDefaultLocale(): AppLocale {
  const saved = localStorage.getItem(LOCALE_STORAGE_KEY) as AppLocale | null
  if (saved && ['zh-CN', 'zh-TW', 'en'].includes(saved)) {
    return saved
  }

  const browserLang = navigator.language
  if (browserLang.startsWith('zh')) {
    return browserLang.includes('TW') || browserLang.includes('HK') ? 'zh-TW' : 'zh-CN'
  }
  return 'en'
}
