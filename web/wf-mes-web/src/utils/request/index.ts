import axios, { type AxiosError, type AxiosRequestConfig, type AxiosResponse, type InternalAxiosRequestConfig } from 'axios'
import { ElMessage } from 'element-plus'
import router from '@/router'
import i18n from '@/i18n'
import type { ApiResult } from '@/types/base/response'
import type { UserInfo } from '@/types/auth/user'
import {
  clearAllStorage,
  getAccessToken,
  getFactoryId,
  getRefreshToken,
  setTokens
} from '@/utils/auth/token'

const request = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  timeout: 30000
})

let isRefreshing = false
let pendingRequests: Array<(token: string) => void> = []

request.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const token = getAccessToken()
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  const factoryId = getFactoryId()
  if (factoryId) {
    config.headers['X-Factory-Id'] = String(factoryId)
  }
  config.headers['Accept-Language'] = i18n.global.locale.value
  return config
})

async function handleApiResult<T>(result: ApiResult<T>, config: InternalAxiosRequestConfig) {
  if (result.code === 200) {
    return result.data as never
  }

  if (result.code === 401 && !config.url?.includes('/auth/login')) {
    if (result.messageCode === 'session.replaced_by_other_device') {
      clearAllStorage()
      router.push('/login')
      ElMessage.warning(result.message)
      return Promise.reject(new Error(result.message)) as never
    }
    return refreshAndRetry(config) as never
  }

  ElMessage.error(result.message || '请求失败')
  return Promise.reject(new Error(result.message)) as never
}

type WfRequestConfig = InternalAxiosRequestConfig & {
  wfRawResponse?: boolean
}

request.interceptors.response.use(
  async (response) => {
    if (response.config.responseType === 'blob') {
      const blob = response.data as Blob
      const contentType = String(response.headers['content-type'] || '')
      if (contentType.includes('application/json')) {
        const text = await blob.text()
        const result = JSON.parse(text) as ApiResult
        return handleApiResult(result, response.config)
      }
      if ((response.config as WfRequestConfig).wfRawResponse) {
        return response as never
      }
      return blob as never
    }

    const result = response.data as ApiResult
    return handleApiResult(result, response.config)
  },
  (error: AxiosError<ApiResult>) => {
    if (error.response?.status === 401) {
      clearAllStorage()
      router.push('/login')
    }
    ElMessage.error(error.response?.data?.message || error.message)
    return Promise.reject(error)
  }
)

async function refreshAndRetry(config: InternalAxiosRequestConfig) {
  const refreshToken = getRefreshToken()
  if (!refreshToken) {
    clearAllStorage()
    router.push('/login')
    throw new Error('未登录')
  }

  if (!isRefreshing) {
    isRefreshing = true
    try {
      const { data } = await axios.post<ApiResult<{ accessToken: string; refreshToken: string; userInfo?: UserInfo }>>(
        `${import.meta.env.VITE_API_BASE_URL}/auth/refresh`,
        { refreshToken }
      )
      if (data.code !== 200) {
        throw new Error(data.message)
      }
      setTokens(data.data.accessToken, data.data.refreshToken, data.data.userInfo?.factoryId)
      pendingRequests.forEach((cb) => cb(data.data.accessToken))
      pendingRequests = []
      config.headers.Authorization = `Bearer ${data.data.accessToken}`
      return await request(config)
    } catch {
      clearAllStorage()
      router.push('/login')
      throw new Error('登录已过期')
    } finally {
      isRefreshing = false
    }
  }

  return new Promise((resolve) => {
    pendingRequests.push(async (token: string) => {
      config.headers.Authorization = `Bearer ${token}`
      resolve(await request(config))
    })
  })
}

export default request

export type { AxiosRequestConfig, AxiosResponse }
