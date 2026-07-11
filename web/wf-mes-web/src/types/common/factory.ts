export interface FactorySummary {
  id: number
  factoryCode: string
  factoryName: string
  isDefault: boolean
}

export interface FactoryItem {
  id?: number
  regionId: number
  regionName?: string
  factoryCode: string
  factoryName: string
  timeZone?: string
  sort: number
  status: number
  remark?: string
  createTime?: string
}

export interface RegionItem {
  id: number
  regionCode: string
  regionName: string
  sort: number
  status: number
}
