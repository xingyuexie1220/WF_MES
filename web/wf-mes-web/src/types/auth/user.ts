import type { FactorySummary } from '@/types/common/factory'

export interface UserInfo {
  id: number
  userName: string
  nickName?: string
  deptId: number
  deptName?: string
  factoryId?: number
  factoryCode?: string
  factoryName?: string
  mustChangePassword?: boolean
  roles: string[]
  permissions: string[]
  accessibleFactories?: FactorySummary[]
}

export interface RouterMenu {
  id: number
  name: string
  path: string
  component?: string
  redirect?: string
  meta: {
    title: string
    i18nKey?: string
    icon?: string
    hidden?: boolean
    permissions?: string[]
  }
  children?: RouterMenu[]
}
