import type { SidebarMenuItem } from '@/utils/menu'
import { useUserStore } from '@/stores/auth/user'

interface FallbackMenuLeaf {
  path: string
  titleKey: string
  permission?: string
  icon?: string
}

interface FallbackMenuGroup {
  id: string
  titleKey: string
  icon?: string
  children: FallbackMenuLeaf[]
}

/** 系统设置 — 组织 / 用户 / 菜单 / 角色 */
const SYSTEM_CORE_ITEMS: FallbackMenuLeaf[] = [
  { path: '/system/dept', titleKey: 'menu.orgStructure', permission: 'system:dept:list', icon: 'OfficeBuilding' },
  { path: '/system/user', titleKey: 'menu.user', permission: 'system:user:list', icon: 'User' },
  { path: '/system/menu', titleKey: 'menu.permission', permission: 'system:menu:list', icon: 'Menu' },
  { path: '/system/role', titleKey: 'menu.role', permission: 'system:role:list', icon: 'UserFilled' }
]

/** 平台配置 */
const SYSTEM_PLATFORM_GROUP: FallbackMenuGroup = {
  id: 'system-platform',
  titleKey: 'menu.platformConfig',
  icon: 'Tools',
  children: [
    { path: '/system/dict', titleKey: 'menu.dict', permission: 'system:dict:list', icon: 'Collection' },
    { path: '/system/notice', titleKey: 'menu.notice', permission: 'system:notice:list', icon: 'Bell' }
  ]
}

/** 系统监控 */
const SYSTEM_MONITOR_GROUP: FallbackMenuGroup = {
  id: 'system-monitor',
  titleKey: 'menu.systemMonitor',
  icon: 'Monitor',
  children: [
    { path: '/system/job', titleKey: 'menu.job', permission: 'system:job:list', icon: 'Timer' },
    { path: '/system/log', titleKey: 'menu.systemLog', permission: 'system:log:list', icon: 'Document' }
  ]
}

const MASTER_DATA_ITEMS: FallbackMenuLeaf[] = [
  { path: '/master-data/material', titleKey: 'menu.masterMaterial', permission: 'master:material:list', icon: 'Box' },
  { path: '/master-data/route', titleKey: 'menu.masterRoute', permission: 'master:route:list', icon: 'Share' },
  { path: '/master-data/station', titleKey: 'menu.masterStation', permission: 'master:station:list', icon: 'Location' },
  {
    path: '/master-data/workcenter',
    titleKey: 'menu.masterWorkCenter',
    permission: 'master:workcenter:list',
    icon: 'OfficeBuilding'
  }
]

const PRODUCTION_ITEMS: FallbackMenuLeaf[] = [
  {
    path: '/production/work-order',
    titleKey: 'menu.productionWorkOrder',
    permission: 'mes:workorder:scan',
    icon: 'Document'
  }
]

const REPORT_ITEMS: FallbackMenuLeaf[] = [
  { path: '/report/output', titleKey: 'menu.reportOutput', permission: 'dashboard:report:view', icon: 'DataLine' },
  { path: '/report/wip', titleKey: 'menu.reportWip', permission: 'dashboard:report:view', icon: 'PieChart' }
]

const BARCODE_ITEMS: FallbackMenuLeaf[] = [
  {
    path: '/barcode/customer',
    titleKey: 'menu.barcodeCustomer',
    permission: 'barcode:customer:list',
    icon: 'User'
  }
]

function mapLeaves(items: FallbackMenuLeaf[]): SidebarMenuItem[] {
  const userStore = useUserStore()
  return items
    .filter((item) => !item.permission || userStore.hasPermission(item.permission))
    .map((item) => ({
      id: item.path,
      index: item.path,
      path: item.path,
      titleKey: item.titleKey,
      icon: item.icon
    }))
}

function mapGroup(group: FallbackMenuGroup): SidebarMenuItem | null {
  const children = mapLeaves(group.children)
  if (children.length === 0) {
    return null
  }
  return {
    id: group.id,
    index: group.id,
    titleKey: group.titleKey,
    icon: group.icon,
    children
  }
}

function buildGroupedMenu(id: string, titleKey: string, icon: string, leaves: FallbackMenuLeaf[]): SidebarMenuItem | null {
  const children = mapLeaves(leaves)
  if (children.length === 0) {
    return null
  }
  return {
    id,
    index: id,
    titleKey,
    icon,
    children
  }
}

export function buildFallbackSystemMenus(): SidebarMenuItem[] {
  const children: SidebarMenuItem[] = [
    ...mapLeaves(SYSTEM_CORE_ITEMS),
    mapGroup(SYSTEM_PLATFORM_GROUP),
    mapGroup(SYSTEM_MONITOR_GROUP)
  ].filter((item): item is SidebarMenuItem => item !== null)

  if (children.length === 0) {
    return []
  }

  return [
    {
      id: 'system',
      index: 'system',
      titleKey: 'menu.system',
      icon: 'Setting',
      children
    }
  ]
}

export function buildMasterDataFallback(): SidebarMenuItem[] {
  const group = buildGroupedMenu('master-data', 'menu.masterData', 'Setting', MASTER_DATA_ITEMS)
  return group ? [group] : []
}

export function buildProductionFallback(): SidebarMenuItem[] {
  const group = buildGroupedMenu('production', 'menu.production', 'Document', PRODUCTION_ITEMS)
  return group ? [group] : []
}

export function buildReportFallback(): SidebarMenuItem[] {
  const group = buildGroupedMenu('report', 'menu.report', 'DataAnalysis', REPORT_ITEMS)
  return group ? [group] : []
}

export function buildBarcodeFallback(): SidebarMenuItem[] {
  const group = buildGroupedMenu('barcode', 'menu.desktop.barcode', 'Printer', BARCODE_ITEMS)
  return group ? [group] : []
}

export function buildFallbackMenusWhenApiEmpty(): SidebarMenuItem[] {
  return [
    ...buildMasterDataFallback(),
    ...buildProductionFallback(),
    ...buildReportFallback(),
    ...buildBarcodeFallback(),
    ...buildFallbackSystemMenus()
  ]
}
