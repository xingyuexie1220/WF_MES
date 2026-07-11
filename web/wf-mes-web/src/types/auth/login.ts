import type { FactorySummary } from '@/types/common/factory'
import type { UserInfo } from '@/types/auth/user'

export interface LoginRequest {
  userName: string
  password: string
  clientType?: number
  locale?: string
  factoryId?: number
}

export interface LoginResponse {
  needSelectFactory?: boolean
  factories?: FactorySummary[]
  accessToken: string
  refreshToken: string
  expiresIn: number
  userInfo: UserInfo
}

export interface SelectFactoryRequest {
  userName: string
  password: string
  factoryId: number
  clientType?: number
  locale?: string
}

export interface ChangePasswordRequest {
  oldPassword: string
  newPassword: string
}

export interface RefreshTokenRequest {
  refreshToken: string
}
