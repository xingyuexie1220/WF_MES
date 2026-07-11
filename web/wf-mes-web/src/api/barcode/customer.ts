import { http } from '@/utils/request/http'
import type {
  BarcodeCustomerDto,
  BarcodeMaterialRuleDto,
  CreatePrintJobRequest,
  PrintJobDto,
  SaveBarcodeCustomerRequest
} from '@/types/barcode/customer'

export const getCustomersApi = () => http.get<BarcodeCustomerDto[]>('/barcode/customers')

export const getCustomerApi = (id: number) => http.get<BarcodeCustomerDto | null>(`/barcode/customers/${id}`)

export const saveCustomerApi = (body: SaveBarcodeCustomerRequest) =>
  http.post<number>('/barcode/customers', body)

export const getMaterialRulesApi = () => http.get<BarcodeMaterialRuleDto[]>('/barcode/material-rules')

export const createPrintJobApi = (body: CreatePrintJobRequest) =>
  http.post<PrintJobDto>('/barcode/print-jobs', body)

export const confirmPrintJobApi = (jobId: string) =>
  http.post<void>(`/barcode/print-jobs/${jobId}/confirm`)
