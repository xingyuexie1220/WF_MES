export interface BarcodeCustomerDto {
  customerId: number
  customerName: string
  enable: number
  createdBy?: string
  createDate: string
  updatedBy?: string
  updatedAt?: string
}

export interface SaveBarcodeCustomerRequest {
  customerId: number
  customerName: string
  enable: number
}

export interface BarcodeMaterialRuleDto {
  ruleId: number
  customerId: number
  materialNo: string
  barcodeLength: number
  qaStatus: number
}

export interface CreatePrintJobRequest {
  ruleId: number
  quantity: number
}

export interface PrintJobDto {
  jobId: string
  status: string
  barcodes: string[]
}
