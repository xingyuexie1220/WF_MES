import { http } from '@/utils/request/http'
import type { PagedResult } from '@/types/base/response'
import type { SessionItem, SessionQuery } from '@/types/system/session'

export const getSessionPageApi = (params: SessionQuery) =>
  http.get<PagedResult<SessionItem>>('/system/sessions', { params })

export const kickSessionApi = (userId: number, clientType: number) =>
  http.post<void>('/system/sessions/kick', { userId, clientType })
