export interface RoleItem {
  id: number
  roleCode: string
  roleName: string
  sort: number
  dataScope: number
  status: number
  remark?: string
  createBy?: number
  createByName?: string
  createTime?: string
  updateBy?: number
  updateByName?: string
  updateTime?: string
  menuIds: number[]
  deptIds: number[]
}
