export interface DictTypeItem {
  id: number
  dictName: string
  dictType: string
  status: number
  remark?: string
  createBy?: number
  createByName?: string
  createTime?: string
  updateBy?: number
  updateByName?: string
  updateTime?: string
}

export interface DictDataItem {
  id: number
  dictTypeId: number
  dictType: string
  dictLabel: string
  dictValue: string
  sort: number
  status: number
  remark?: string
  createBy?: number
  createByName?: string
  createTime?: string
  updateBy?: number
  updateByName?: string
  updateTime?: string
}
