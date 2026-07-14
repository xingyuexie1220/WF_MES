export interface MesProcessProgress {
  processCode: string
  processName?: string
  seq: number
  goodQty: number
  defectQty: number
  remainQty: number
}

export interface WorkOrderDto {
  workOrderId: number
  workOrderNo: string
  materialNo: string
  materialName?: string
  routingId?: number
  routingName?: string
  planQty: number
  dueDate?: string
  status: string
  source: string
  progress: MesProcessProgress[]
  [key: string]: unknown
}

export interface PassStationRequest {
  workOrderNo?: string
  barcode?: string
  processCode?: string
  stationCode?: string
  goodQty?: number
  defectQty?: number
  defectCode?: string
  disposition?: string
  reworkToProcess?: string
  machineNo?: string
  [key: string]: unknown
}

export interface SaveWorkOrderRequest {
  id?: number
  workOrderNo: string
  materialNo: string
  routingId?: number
  planQty: number
  dueDate?: string
  remark?: string
}
