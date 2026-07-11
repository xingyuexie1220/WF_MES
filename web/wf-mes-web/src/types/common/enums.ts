/** 1=Web 2=Mobile 3=Desktop */
export const CLIENT_WEB = 1
export const CLIENT_MOBILE = 2
export const CLIENT_DESKTOP = 3

export const DataScopeType = {
  All: 1,
  Custom: 2,
  Dept: 3,
  DeptAndChild: 4,
  Self: 5
} as const

export const DeptType = {
  Workshop: 1,
  Line: 2,
  Team: 3
} as const

export type DataScopeTypeValue = (typeof DataScopeType)[keyof typeof DataScopeType]
export type DeptTypeValue = (typeof DeptType)[keyof typeof DeptType]
