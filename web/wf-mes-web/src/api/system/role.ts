import { http } from '@/utils/request/http'
import type { PagedResult } from '@/types/base/response'
import type { RoleItem } from '@/types/system/role'

export const getRolePageApi = (params: Record<string, unknown>) =>
  http.get<PagedResult<RoleItem>>('/system/roles', { params })

export const getAllRolesApi = () => http.get<RoleItem[]>('/system/roles/all')
export const createRoleApi = (data: Record<string, unknown>) => http.post<number>('/system/roles', data)
export const updateRoleApi = (id: number, data: Record<string, unknown>) =>
  http.post<void>(`/system/roles/update/${id}`, data)
export const deleteRoleApi = (id: number) => http.post<void>(`/system/roles/delete/${id}`)
