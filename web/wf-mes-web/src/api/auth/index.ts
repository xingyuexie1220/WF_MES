import { http } from '@/utils/request/http'
import type {
  ChangePasswordRequest,
  LoginRequest,
  LoginResponse,
  SelectFactoryRequest
} from '@/types/auth/login'
import type { RouterMenu, UserInfo } from '@/types/auth/user'

export const loginApi = (data: LoginRequest) => http.post<LoginResponse>('/auth/login', data)
export const selectFactoryApi = (data: SelectFactoryRequest) =>
  http.post<LoginResponse>('/auth/select-factory', data)
export const switchFactoryApi = (data: { factoryId: number }) =>
  http.post<LoginResponse>('/auth/switch-factory', data)
export const changePasswordApi = (data: ChangePasswordRequest) =>
  http.post<UserInfo>('/auth/change-password', data)
export const logoutApi = () => http.post<void>('/auth/logout')
export const getUserInfoApi = () => http.get<UserInfo>('/auth/info')
export const getRoutersApi = () => http.get<RouterMenu[]>('/auth/routers')
