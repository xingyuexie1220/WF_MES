const ACCESS_TOKEN_KEY = 'wf_access_token'
const REFRESH_TOKEN_KEY = 'wf_refresh_token'
const FACTORY_ID_KEY = 'wf_factory_id'

export function getAccessToken() {
  return localStorage.getItem(ACCESS_TOKEN_KEY) || ''
}

export function getRefreshToken() {
  return localStorage.getItem(REFRESH_TOKEN_KEY) || ''
}

export function getFactoryId() {
  const value = localStorage.getItem(FACTORY_ID_KEY)
  return value ? Number(value) : undefined
}

export function setTokens(accessToken: string, refreshToken: string, factoryId?: number) {
  localStorage.setItem(ACCESS_TOKEN_KEY, accessToken)
  localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken)
  if (factoryId) {
    localStorage.setItem(FACTORY_ID_KEY, String(factoryId))
  }
}

export function setFactoryId(factoryId: number) {
  localStorage.setItem(FACTORY_ID_KEY, String(factoryId))
}

export function clearAuthStorage() {
  localStorage.removeItem(ACCESS_TOKEN_KEY)
  localStorage.removeItem(REFRESH_TOKEN_KEY)
  localStorage.removeItem(FACTORY_ID_KEY)
}

export function clearAllStorage() {
  localStorage.clear()
}
