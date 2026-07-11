export interface DeptItem {
  id: number
  factoryId?: number
  parentId: number
  deptCode: string
  deptName: string
  deptType: number
  sort: number
  status: number
  remark?: string
  createTime?: string
  createBy?: number
  createByName?: string
  updateTime?: string
  updateBy?: number
  updateByName?: string
  children?: DeptItem[]
}
