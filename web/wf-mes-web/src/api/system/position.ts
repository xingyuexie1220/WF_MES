import { http } from '@/utils/request/http'
import type { PagedResult } from '@/types/base/response'
import type { PositionItem } from '@/types/system/position'

export const getPositionPageApi = (params: Record<string, unknown>) =>
  http.get<PagedResult<PositionItem>>('/system/positions', { params })

export const getAllPositionsApi = () => http.get<PositionItem[]>('/system/positions/all')
export const createPositionApi = (data: Record<string, unknown>) => http.post<void>('/system/positions', data)
export const updatePositionApi = (id: number, data: Record<string, unknown>) =>
  http.post<void>(`/system/positions/update/${id}`, data)
export const deletePositionApi = (id: number) => http.post<void>(`/system/positions/delete/${id}`)
