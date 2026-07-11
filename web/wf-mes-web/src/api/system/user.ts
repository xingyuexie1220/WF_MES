import { http } from '@/utils/request/http'
import type { PagedResult } from '@/types/base/response'
import type { UserItem } from '@/types/system/user'

export const getUserPageApi = (params: Record<string, unknown>) =>
  http.get<PagedResult<UserItem>>('/system/users', { params })

export const getUserByIdApi = (id: number) => http.get<UserItem>(`/system/users/${id}`)

export const createUserApi = (data: Record<string, unknown>) => http.post<number>('/system/users', data)
export const updateUserApi = (id: number, data: Record<string, unknown>) =>
  http.post<void>(`/system/users/update/${id}`, data)
export const deleteUserApi = (id: number) => http.post<void>(`/system/users/delete/${id}`)
export const resetPasswordApi = (id: number, newPassword: string) =>
  http.post<void>(`/system/users/reset-password/${id}`, { newPassword })
