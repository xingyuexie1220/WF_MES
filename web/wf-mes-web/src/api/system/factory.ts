import { http } from '@/utils/request/http'
import type { FactoryItem, RegionItem } from '@/types/common/factory'

export const getFactoryRegionsApi = () => http.get<RegionItem[]>('/system/factories/regions')
export const getFactoryListApi = () => http.get<FactoryItem[]>('/system/factories')
export const createFactoryApi = (data: Partial<FactoryItem>) => http.post<number>('/system/factories', data)
export const updateFactoryApi = (id: number, data: Partial<FactoryItem>) =>
  http.put<void>(`/system/factories/${id}`, data)
export const deleteFactoryApi = (id: number) => http.delete<void>(`/system/factories/${id}`)
