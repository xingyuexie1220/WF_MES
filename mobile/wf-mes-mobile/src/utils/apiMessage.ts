import { t } from '@/i18n'

export interface ApiMessageSource {
  messageCode?: string
  message?: string
}

/** 优先用 messageCode 翻译；message 仅作 fallback（兼容旧 API）。 */
export function resolveApiMessage(
  result: ApiMessageSource,
  fallbackKey = 'common.requestFailed'
): string {
  if (result.messageCode) {
    const translated = t(result.messageCode)
    if (translated && translated !== result.messageCode) {
      return translated
    }
  }

  if (result.message) {
    return result.message
  }

  return t(fallbackKey)
}
