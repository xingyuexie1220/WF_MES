/** 后端 menu.icon 多为语义 key（home/scan/list…），映射为色块 + uView 图标名。 */

export interface MenuIconStyle {
  bg: string
  fg: string
  /** uview-plus u-icon name */
  name: string
}

const ICON_MAP: Record<string, MenuIconStyle> = {
  home: { bg: '#dbeafe', fg: '#2563eb', name: 'home' },
  phone: { bg: '#ede9fe', fg: '#7c3aed', name: 'phone' },
  scan: { bg: '#ffedd5', fg: '#ea580c', name: 'scan' },
  list: { bg: '#e0e7ff', fg: '#4f46e5', name: 'list' },
  finished: { bg: '#dcfce7', fg: '#16a34a', name: 'checkmark' },
  pass: { bg: '#dcfce7', fg: '#16a34a', name: 'checkmark' },
  warehouse: { bg: '#ffedd5', fg: '#ea580c', name: 'bag' },
  inventory: { bg: '#e0e7ff', fg: '#4f46e5', name: 'file-text' },
  grid: { bg: '#f1f5f9', fg: '#64748b', name: 'grid' }
}

const DEFAULT_ICON: MenuIconStyle = { bg: '#f1f5f9', fg: '#64748b', name: 'grid' }

export function resolveMenuIcon(icon?: string | null, path?: string | null): MenuIconStyle {
  const key = (icon || '').trim().toLowerCase()
  if (key && ICON_MAP[key]) {
    return ICON_MAP[key]
  }

  const p = (path || '').toLowerCase()
  if (p.includes('warehouse') || (p.includes('/scan') && !p.includes('/pages/scan'))) {
    return ICON_MAP.warehouse
  }
  if (p.includes('inventory') || p.includes('/list')) return ICON_MAP.inventory
  if (p.includes('simple-pass') || p.includes('/pass') || p.includes('mes/report')) return ICON_MAP.finished
  if (p.includes('/home')) return ICON_MAP.home
  if (p.includes('/mine')) return ICON_MAP.phone

  return DEFAULT_ICON
}

export function isTabMenuPath(path?: string | null): boolean {
  if (!path) return false
  const p = path.replace(/\\/g, '/')
  return (
    p.includes('/pages/home/') ||
    p.includes('/pages/scan/') ||
    p.includes('/pages/mine/') ||
    /\/pages\/(home|scan|mine)(\/index)?$/.test(p)
  )
}

export function isDevelopingPath(path?: string | null): boolean {
  if (!path) return false
  return path.toLowerCase().includes('inventory')
}

/** 扫码 Tab「扫码业务」：仅展示扫码类菜单，且须已授权。 */
export function isScanBizPath(path?: string | null): boolean {
  if (!path) return false
  const p = path.toLowerCase().replace(/\\/g, '/')
  return p.includes('warehouse/scan') || p.includes('simple-pass') || p.includes('mes/report')
}
