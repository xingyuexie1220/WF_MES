import { http } from '@/utils/request/http'
import type { DeptItem } from '@/types/system/dept'

export const getDeptTreeApi = () => http.get<DeptItem[]>('/system/depts/tree')
export const getDeptByIdApi = (id: number) => http.get<DeptItem>(`/system/depts/${id}`)
export const createDeptApi = (data: Record<string, unknown>) => http.post<void>('/system/depts', data)
export const updateDeptApi = (id: number, data: Record<string, unknown>) =>
  http.post<void>(`/system/depts/update/${id}`, data)
export const deleteDeptApi = (id: number) => http.post<void>(`/system/depts/delete/${id}`)
