import { createI18n } from 'vue-i18n'
import zhCN from '../../../../i18n/messages/zh-CN.json'
import en from '../../../../i18n/messages/en.json'
import zhTW from '../../../../i18n/messages/zh-TW.json'
import { getLocale, STORAGE_KEYS } from '@/utils/auth'

export type AppLocale = 'zh-CN' | 'zh-TW' | 'en'

export const localeOptions: Array<{ labelKey: string; value: AppLocale }> = [
  { labelKey: 'locale.zhCN', value: 'zh-CN' },
  { labelKey: 'locale.zhTW', value: 'zh-TW' },
  { labelKey: 'locale.en', value: 'en' }
]

export const i18n = createI18n({
  legacy: false,
  locale: getLocale(),
  fallbackLocale: 'zh-CN',
  messages: {
    'zh-CN': zhCN,
    en,
    'zh-TW': zhTW
  }
})

export function t(key: string) {
  return i18n.global.t(key)
}

export function setAppLocale(locale: AppLocale) {
  i18n.global.locale.value = locale
  uni.setStorageSync(STORAGE_KEYS.locale, locale)
  uni.setTabBarItem({ index: 0, text: t('mobile.tab.home') })
  uni.setTabBarItem({ index: 1, text: t('mobile.tab.scan') })
  uni.setTabBarItem({ index: 2, text: t('mobile.tab.mine') })
}

export function useI18n() {
  return i18n.global
}
