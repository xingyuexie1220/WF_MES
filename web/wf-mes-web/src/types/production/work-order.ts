export interface WorkOrderDto {
  workOrderId?: number
  workOrderNo?: string
  materialNo?: string
  status?: string
  qty?: number
  [key: string]: unknown
}

export interface PassStationRequest {
  workOrderNo: string
  stationCode: string
  [key: string]: unknown
}
