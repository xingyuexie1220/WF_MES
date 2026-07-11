export interface MenuItem {
  id: number
  parentId: number
  menuName: string
  menuType: number
  clientType: number
  i18nKey?: string
  path?: string
  component?: string
  permission?: string
  icon?: string
  sort: number
  visible: boolean
  status: number
  remark?: string
  createTime?: string
  createBy?: number
  createByName?: string
  updateTime?: string
  updateBy?: number
  updateByName?: string
  children?: MenuItem[]
}
