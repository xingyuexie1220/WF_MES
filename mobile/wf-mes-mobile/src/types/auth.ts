export interface FactorySummary {
  id: number
  factoryCode: string
  factoryName: string
  isDefault: boolean
}

export interface UserInfo {
  id: number
  userName: string
  nickName?: string
  factoryId?: number
  factoryCode?: string
  factoryName?: string
  mustChangePassword?: boolean
  roles?: string[]
  permissions?: string[]
  accessibleFactories?: FactorySummary[]
}

export interface LoginResult {
  needSelectFactory?: boolean
  factories?: FactorySummary[]
  accessToken: string
  refreshToken: string
  userInfo: UserInfo
}
