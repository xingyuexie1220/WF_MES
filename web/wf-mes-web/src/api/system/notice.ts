import { http } from '@/utils/request/http'
import type { PagedResult } from '@/types/base/response'
import type { NoticeItem, NoticePushItem } from '@/types/system/notice'

export const getNoticePageApi = (params: Record<string, unknown>) =>
  http.get<PagedResult<NoticeItem>>('/system/notices', { params })

export const getPublishedNoticesApi = (count = 20) =>
  http.get<NoticePushItem[]>('/system/notices/published', { params: { count } })

export const createNoticeApi = (data: Record<string, unknown>) => http.post<number>('/system/notices', data)

export const updateNoticeApi = (id: number, data: Record<string, unknown>) =>
  http.post<void>(`/system/notices/update/${id}`, data)

export const deleteNoticeApi = (id: number) => http.post<void>(`/system/notices/delete/${id}`)

export const publishNoticeApi = (id: number) => http.post<void>(`/system/notices/publish/${id}`)
