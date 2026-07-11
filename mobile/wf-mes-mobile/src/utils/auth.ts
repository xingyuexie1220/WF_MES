import type { UserInfo } from '@/types/auth'

export const STORAGE_KEYS = {
  accessToken: 'wf_access_token',
  refreshToken: 'wf_refresh_token',
  factoryId: 'wf_factory_id',
  factoryCode: 'wf_factory_code',
  factoryName: 'wf_factory_name',
  mustChangePassword: 'wf_must_change_password',
  locale: 'wf_locale',
  userInfo: 'wf_user_info'
} as const

export function getAccessToken() {
  return uni.getStorageSync(STORAGE_KEYS.accessToken) || ''
}

export function getRefreshToken() {
  return uni.getStorageSync(STORAGE_KEYS.refreshToken) || ''
}

export function getFactoryId(): number | undefined {
  const value = uni.getStorageSync(STORAGE_KEYS.factoryId)
  return value ? Number(value) : undefined
}

export function getLocale() {
  return uni.getStorageSync(STORAGE_KEYS.locale) || 'zh-CN'
}

export function getStoredUserInfo(): UserInfo | null {
  const raw = uni.getStorageSync(STORAGE_KEYS.userInfo)
  if (!raw) return null
  try {
    return typeof raw === 'string' ? JSON.parse(raw) : raw
  } catch {
    return null
  }
}

export function setTokens(accessToken: string, refreshToken: string, factoryId?: number) {
  uni.setStorageSync(STORAGE_KEYS.accessToken, accessToken)
  uni.setStorageSync(STORAGE_KEYS.refreshToken, refreshToken)
  if (factoryId) {
    uni.setStorageSync(STORAGE_KEYS.factoryId, factoryId)
  }
}

export function setUserInfo(userInfo: UserInfo) {
  uni.setStorageSync(STORAGE_KEYS.userInfo, JSON.stringify(userInfo))
  if (userInfo.factoryId) {
    uni.setStorageSync(STORAGE_KEYS.factoryId, userInfo.factoryId)
  }
  if (userInfo.factoryCode) {
    uni.setStorageSync(STORAGE_KEYS.factoryCode, userInfo.factoryCode)
  }
  if (userInfo.factoryName) {
    uni.setStorageSync(STORAGE_KEYS.factoryName, userInfo.factoryName)
  }
  uni.setStorageSync(STORAGE_KEYS.mustChangePassword, userInfo.mustChangePassword ? 1 : 0)
}

export function clearAuthStorage() {
  Object.values(STORAGE_KEYS).forEach((key) => uni.removeStorageSync(key))
}

export function isLoggedIn() {
  return !!getAccessToken()
}

export function mustChangePassword() {
  return uni.getStorageSync(STORAGE_KEYS.mustChangePassword) === 1
}
