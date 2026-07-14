export interface MesProcessItem {
  id: number
  processCode: string
  processName: string
  defaultSeq: number
  enabled: boolean
  remark?: string
}

export interface MesRoutingStep {
  processCode: string
  processName?: string
  seq: number
}

export interface MesRoutingItem {
  id: number
  routingCode: string
  routingName: string
  materialNo?: string
  enabled: boolean
  remark?: string
  steps: MesRoutingStep[]
}

export interface MesMaterialItem {
  id: number
  materialNo: string
  materialName: string
  spec?: string
  unit?: string
  source: string
  enabled: boolean
}

export interface MesMachineItem {
  id: number
  machineNo: string
  machineName: string
  enabled: boolean
  remark?: string
}

export interface MesDefectCodeItem {
  id: number
  defectCode: string
  defectName: string
  sort: number
  enabled: boolean
}
