import { http } from '@/utils/request/http'
import type { PagedResult } from '@/types/base/response'
import type { ExceptionLogItem, OperationLogItem } from '@/types/system/log'

export const getOperationLogPageApi = (params: Record<string, unknown>) =>
  http.get<PagedResult<OperationLogItem>>('/system/logs/operations', { params })

export const getExceptionLogPageApi = (params: Record<string, unknown>) =>
  http.get<PagedResult<ExceptionLogItem>>('/system/logs/exceptions', { params })

export const exportOperationLogsApi = (params: Record<string, unknown>) =>
  http.get<OperationLogItem[]>('/system/logs/operations/export', { params })

export const exportExceptionLogsApi = (params: Record<string, unknown>) =>
  http.get<ExceptionLogItem[]>('/system/logs/exceptions/export', { params })

export const clearOperationLogsApi = (data?: Record<string, unknown>) =>
  http.post<void>('/system/logs/operations/clear', data ?? {})

export const clearExceptionLogsApi = (data?: Record<string, unknown>) =>
  http.post<void>('/system/logs/exceptions/clear', data ?? {})
