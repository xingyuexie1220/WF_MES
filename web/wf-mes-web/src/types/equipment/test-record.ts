export interface EquipmentTestRecordDto {
  recordId?: number
  equipmentCode?: string
  result?: string
  testTime?: string
  [key: string]: unknown
}

export interface SubmitEquipmentTestRequest {
  equipmentCode: string
  result: string
  [key: string]: unknown
}
