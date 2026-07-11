export interface UserItem {
  id: number
  userName: string
  nickName?: string
  email?: string
  deptId: number
  deptName?: string
  status: number
  remark?: string
  createTime: string
  createBy?: number
  createByName?: string
  updateTime?: string
  updateBy?: number
  updateByName?: string
  roleIds: number[]
  positionIds: number[]
}

import type { PageQuery } from '@/types/base/response'

export interface UserQuery extends PageQuery {
  userName?: string
  status?: number
  deptId?: number
}
