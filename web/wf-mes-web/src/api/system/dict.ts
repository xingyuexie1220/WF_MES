import { http } from '@/utils/request/http'
import type { PagedResult } from '@/types/base/response'
import type { DictDataItem, DictTypeItem } from '@/types/system/dict'

export const getDictTypePageApi = (params: Record<string, unknown>) =>
  http.get<PagedResult<DictTypeItem>>('/system/dicts/types', { params })

export const getAllDictTypesApi = () => http.get<DictTypeItem[]>('/system/dicts/types/all')

export const createDictTypeApi = (data: Record<string, unknown>) => http.post<number>('/system/dicts/types', data)

export const updateDictTypeApi = (id: number, data: Record<string, unknown>) =>
  http.post<void>(`/system/dicts/types/update/${id}`, data)

export const deleteDictTypeApi = (id: number) => http.post<void>(`/system/dicts/types/delete/${id}`)

export const getDictDataPageApi = (params: Record<string, unknown>) =>
  http.get<PagedResult<DictDataItem>>('/system/dicts/data', { params })

export const getDictDataByTypeApi = (dictType: string) =>
  http.get<DictDataItem[]>(`/system/dicts/data/by-type/${dictType}`)

export const getDictOptionsApi = (dictType: string) =>
  http.get<DictDataItem[]>(`/system/dicts/data/options/${dictType}`)

export const createDictDataApi = (data: Record<string, unknown>) => http.post<number>('/system/dicts/data', data)

export const updateDictDataApi = (id: number, data: Record<string, unknown>) =>
  http.post<void>(`/system/dicts/data/update/${id}`, data)

export const deleteDictDataApi = (id: number) => http.post<void>(`/system/dicts/data/delete/${id}`)
