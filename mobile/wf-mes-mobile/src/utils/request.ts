import { appConfig } from '@/config/app'
import { clearAuthStorage, getAccessToken, getFactoryId, getRefreshToken, getLocale, setTokens, setUserInfo } from '@/utils/auth'
import { refreshTokenApi } from '@/api/auth'
import { t } from '@/i18n'

interface ApiResult<T> {
  code: number
  message: string
  messageCode?: string
  data: T
}

interface RequestOptions {
  silent?: boolean
  skipAuth?: boolean
}

let refreshing = false
let pendingQueue: Array<(token: string) => void> = []

function redirectToLogin() {
  clearAuthStorage()
  uni.reLaunch({ url: '/pages/login/index' })
}

async function refreshAccessToken() {
  const refreshToken = getRefreshToken()
  if (!refreshToken) {
    redirectToLogin()
    throw new Error('not logged in')
  }

  if (!refreshing) {
    refreshing = true
    try {
      const data = await refreshTokenApi(refreshToken)
      setTokens(data.accessToken, data.refreshToken, data.userInfo.factoryId)
      setUserInfo(data.userInfo)
      pendingQueue.forEach((cb) => cb(data.accessToken))
      pendingQueue = []
      return data.accessToken
    } catch {
      redirectToLogin()
      throw new Error('refresh failed')
    } finally {
      refreshing = false
    }
  }

  return new Promise<string>((resolve) => {
    pendingQueue.push(resolve)
  })
}

function sendRequest<T>(
  url: string,
  method: 'GET' | 'POST',
  data: Record<string, unknown> | undefined,
  options: RequestOptions,
  token: string
): Promise<T> {
  const factoryId = getFactoryId()
  return new Promise((resolve, reject) => {
    uni.request({
      url: `${appConfig.apiBaseUrl}${url}`,
      method,
      data,
      header: {
        'Content-Type': 'application/json',
        ...(options.skipAuth || !token ? {} : { Authorization: `Bearer ${token}` }),
        'Accept-Language': getLocale(),
        ...(factoryId ? { 'X-Factory-Id': String(factoryId) } : {})
      },
      success: async (res) => {
        const result = res.data as ApiResult<T>
        if (result.code === 200) {
          resolve(result.data)
          return
        }

        if (result.code === 401) {
          if (result.messageCode === 'session.replaced_by_other_device') {
            if (!options.silent) {
              uni.showToast({ title: result.message, icon: 'none' })
            }
            redirectToLogin()
            reject(result)
            return
          }

          if (!options.skipAuth && !url.includes('/auth/refresh')) {
            try {
              const newToken = await refreshAccessToken()
              const retryData = await sendRequest<T>(url, method, data, options, newToken)
              resolve(retryData)
            } catch (error) {
              reject(error)
            }
            return
          }
        }

        if (!options.silent) {
          uni.showToast({ title: result.message || t('common.requestFailed'), icon: 'none' })
        }
        reject(result)
      },
      fail: (error) => {
        if (!options.silent) {
          uni.showToast({ title: t('common.networkError'), icon: 'none' })
        }
        reject(error)
      }
    })
  })
}

export function request<T>(
  url: string,
  method: 'GET' | 'POST' = 'GET',
  data?: Record<string, unknown>,
  options: RequestOptions = {}
): Promise<T> {
  const token = options.skipAuth ? '' : getAccessToken()
  return sendRequest<T>(url, method, data, options, token)
}
