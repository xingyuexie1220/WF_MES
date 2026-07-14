import { http } from '@/utils/request/http'
import type {
  MesDefectCodeItem,
  MesMachineItem,
  MesMaterialItem,
  MesProcessItem,
  MesRoutingItem
} from '@/types/master-data/mes'

export const getMesMaterialsApi = () => http.get<MesMaterialItem[]>('/master-data/materials')
export const saveMesMaterialApi = (data: Partial<MesMaterialItem>) =>
  http.post<number>('/master-data/materials', data)
export const deleteMesMaterialApi = (id: number) =>
  http.post(`/master-data/materials/delete/${id}`)

export const getMesRoutingsApi = () => http.get<MesRoutingItem[]>('/master-data/routes')
export const saveMesRoutingApi = (data: Partial<MesRoutingItem>) =>
  http.post<number>('/master-data/routes', data)
export const deleteMesRoutingApi = (id: number) =>
  http.post(`/master-data/routes/delete/${id}`)

export const getMesProcessesApi = () => http.get<MesProcessItem[]>('/master-data/stations')
export const saveMesProcessApi = (data: Partial<MesProcessItem>) =>
  http.post<number>('/master-data/stations', data)
export const deleteMesProcessApi = (id: number) =>
  http.post(`/master-data/stations/delete/${id}`)

export const getMesMachinesApi = () => http.get<MesMachineItem[]>('/master-data/work-centers')
export const saveMesMachineApi = (data: Partial<MesMachineItem>) =>
  http.post<number>('/master-data/work-centers', data)
export const deleteMesMachineApi = (id: number) =>
  http.post(`/master-data/work-centers/delete/${id}`)

export const getMesDefectCodesApi = () => http.get<MesDefectCodeItem[]>('/master-data/defect-codes')
