import { request } from '@/utils/request'
import { appConfig } from '@/config/app'
import { getLocale } from '@/utils/auth'
import type { LoginResult, UserInfo } from '@/types/auth'

const withLocale = () => getLocale()

export function loginApi(userName: string, password: string, factoryId?: number) {
  return request<LoginResult>(
    '/auth/login',
    'POST',
    {
      userName,
      password,
      clientType: appConfig.clientType,
      locale: withLocale(),
      factoryId
    },
    { skipAuth: true }
  )
}

export function selectFactoryApi(userName: string, password: string, factoryId: number) {
  return request<LoginResult>(
    '/auth/select-factory',
    'POST',
    {
      userName,
      password,
      factoryId,
      clientType: appConfig.clientType,
      locale: withLocale()
    },
    { skipAuth: true }
  )
}

export function switchFactoryApi(factoryId: number) {
  return request<LoginResult>('/auth/switch-factory', 'POST', { factoryId })
}

export function refreshTokenApi(refreshToken: string) {
  return request<LoginResult>('/auth/refresh', 'POST', { refreshToken }, { silent: true, skipAuth: true })
}

export function logoutApi() {
  return request<void>('/auth/logout', 'POST', {}, { silent: true })
}

export function getUserInfoApi() {
  return request<UserInfo>('/auth/info', 'GET')
}

export function changePasswordApi(oldPassword: string, newPassword: string) {
  return request<UserInfo>('/auth/change-password', 'POST', { oldPassword, newPassword })
}

export function getMobileMenusApi() {
  return request<Array<{
    title?: string
    i18nKey?: string
    path?: string
    icon?: string
    children?: unknown[]
  }>>('/auth/mobile-menus', 'GET')
}
