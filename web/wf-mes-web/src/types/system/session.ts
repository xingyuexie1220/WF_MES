export interface SessionItem {
  userId: number
  userName: string
  nickName?: string
  clientType: number
  sessionId: string
  loginTime: string
  expireTime: string
  isActive: boolean
}

import type { PageQuery } from '@/types/base/response'

export interface SessionQuery extends PageQuery {
  userName?: string
  clientType?: number
  onlyActive?: boolean
}
