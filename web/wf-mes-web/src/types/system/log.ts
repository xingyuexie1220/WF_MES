export interface OperationLogItem {
  id: number
  module?: string
  title?: string
  businessType?: string
  method?: string
  requestMethod?: string
  operUrl?: string
  operIp?: string
  operParam?: string
  status: number
  errorMsg?: string
  operUserName?: string
  operTime: string
  costTime: number
}

export interface ExceptionLogItem {
  id: number
  module?: string
  message: string
  stackTrace?: string
  requestUrl?: string
  requestMethod?: string
  requestParam?: string
  operIp?: string
  operUserName?: string
  exceptionTime: string
}
