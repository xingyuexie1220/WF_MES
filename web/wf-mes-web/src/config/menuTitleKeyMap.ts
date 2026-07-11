/**
 * 菜单 path → i18n titleKey 映射。
 * 登录后侧边栏来自 API（MenuName 为中文），展示时优先用 titleKey 走 vue-i18n，切换语言才生效。
 * 数据库仍保留 MenuName（中文），用于菜单管理页展示与运维识别；不必在库里存英文。
 */
const MENU_TITLE_KEY_BY_PATH: Record<string, string> = {
  '/system': 'menu.system',
  '/system/dept': 'menu.orgStructure',
  '/system/user': 'menu.user',
  '/system/menu': 'menu.permission',
  '/system/role': 'menu.role',
  '/system/position': 'menu.position',
  '/system/platform': 'menu.platformConfig',
  '/system/dict': 'menu.dict',
  '/system/notice': 'menu.notice',
  '/system/monitor': 'menu.systemMonitor',
  '/system/job': 'menu.job',
  '/system/log': 'menu.systemLog',
  '/system/session': 'menu.session',
  '/system/factory': 'menu.factory',
  '/master-data': 'menu.masterData',
  '/master-data/material': 'menu.masterMaterial',
  '/master-data/route': 'menu.masterRoute',
  '/master-data/station': 'menu.masterStation',
  '/master-data/workcenter': 'menu.masterWorkCenter',
  '/report': 'menu.report',
  '/report/output': 'menu.reportOutput',
  '/report/wip': 'menu.reportWip',
  '/bigscreen': 'menu.bigscreen',
  '/barcode/customer': 'menu.barcodeCustomer'
}

export function normalizeMenuPath(path?: string): string {
  if (!path) return ''
  const trimmed = path.trim()
  if (!trimmed) return ''
  return trimmed.startsWith('/') ? trimmed.replace(/\/+$/, '') || '/' : `/${trimmed}`.replace(/\/+$/, '')
}

export function resolveMenuTitleKey(path?: string): string | undefined {
  const normalized = normalizeMenuPath(path)
  return normalized ? MENU_TITLE_KEY_BY_PATH[normalized] : undefined
}
