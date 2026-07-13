import { http } from '@/utils/request/http'
import type { BarcodeCustomerDto, SaveBarcodeCustomerRequest } from '@/types/barcode/customer'

export const getCustomersApi = () => http.get<BarcodeCustomerDto[]>('/barcode/customers')

export const saveCustomerApi = (body: SaveBarcodeCustomerRequest) =>
  http.post<number>('/barcode/customers', body)
