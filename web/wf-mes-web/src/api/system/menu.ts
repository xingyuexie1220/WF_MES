import { http } from '@/utils/request/http'
import type { MenuItem } from '@/types/system/menu'

export const getMenuTreeApi = (clientType?: number) =>
  http.get<MenuItem[]>('/system/menus/tree', { params: clientType ? { clientType } : undefined })

export const createMenuApi = (data: Record<string, unknown>) => http.post<void>('/system/menus', data)
export const updateMenuApi = (id: number, data: Record<string, unknown>) =>
  http.post<void>(`/system/menus/update/${id}`, data)
export const deleteMenuApi = (id: number) => http.post<void>(`/system/menus/delete/${id}`)
