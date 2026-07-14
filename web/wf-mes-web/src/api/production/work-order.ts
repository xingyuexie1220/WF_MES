import { http } from '@/utils/request/http'
import type { PassStationRequest, SaveWorkOrderRequest, WorkOrderDto } from '@/types/production/work-order'

export const getWorkOrdersApi = (status?: string) =>
  http.get<WorkOrderDto[]>('/production/work-orders', { params: status ? { status } : undefined })

export const getWorkOrderApi = (workOrderNo: string) =>
  http.get<WorkOrderDto>(`/production/work-orders/${encodeURIComponent(workOrderNo)}`)

export const saveWorkOrderApi = (body: SaveWorkOrderRequest) =>
  http.post<number>('/production/work-orders', body)

export const closeWorkOrderApi = (id: number, remark?: string) =>
  http.post(`/production/work-orders/${id}/close`, { remark })

export const passStationApi = (body: PassStationRequest) =>
  http.post<unknown>('/production/pass-records', body)
