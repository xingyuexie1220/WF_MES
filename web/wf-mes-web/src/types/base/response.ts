export interface ApiResult<T = unknown> {
  code: number
  message: string
  messageCode?: string
  messageArgs?: Record<string, unknown>
  data: T
  timestamp: number
}

export interface PagedResult<T> {
  pageIndex: number
  pageSize: number
  total: number
  items: T[]
}

export interface PageQuery {
  pageIndex?: number
  pageSize?: number
}
