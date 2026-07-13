import i18n from '@/i18n'

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
    const translated = i18n.global.t(result.messageCode)
    if (translated !== result.messageCode) {
      return String(translated)
    }
  }

  if (result.message) {
    return result.message
  }

  return String(i18n.global.t(fallbackKey))
}
