import { http } from '@/utils/request/http'
import type { PassStationRequest, WorkOrderDto } from '@/types/production/work-order'

export const getWorkOrdersApi = () => http.get<WorkOrderDto[]>('/production/work-orders')

export const passStationApi = (body: PassStationRequest) =>
  http.post<unknown>('/production/pass-records', body)
