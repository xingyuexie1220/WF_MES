import { http } from '@/utils/request/http'
import type { EquipmentTestRecordDto, SubmitEquipmentTestRequest } from '@/types/equipment/test-record'

export const getEquipmentTestRecordsApi = () =>
  http.get<EquipmentTestRecordDto[]>('/equipment/test-records')

export const submitEquipmentTestApi = (body: SubmitEquipmentTestRequest) =>
  http.post<unknown>('/equipment/test-records', body)
