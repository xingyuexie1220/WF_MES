import type { Component } from 'vue'
import * as ElementPlusIconsVue from '@element-plus/icons-vue'
import { Setting } from '@element-plus/icons-vue'
import { resolveMenuTitleKey } from '@/config/menuTitleKeyMap'
import type { RouterMenu } from '@/types/auth/user'

export interface SidebarMenuItem {
  id: string
  index: string
  path?: string
  title?: string
  titleKey?: string
  icon?: string
  placeholder?: boolean
  children?: SidebarMenuItem[]
}

export function joinMenuPath(parentPath: string, path: string): string {
  if (!path) {
    return parentPath
  }
  if (path.startsWith('http://') || path.startsWith('https://')) {
    return path
  }
  if (path.startsWith('/')) {
    return path
  }
  const base = parentPath.replace(/\/$/, '')
  return `${base}/${path}`.replace(/\/+/g, '/')
}

export function mapRouterMenus(menus: RouterMenu[], parentPath = ''): SidebarMenuItem[] {
  return menus
    .filter((menu) => !menu.meta.hidden)
    .map((menu) => {
      const fullPath = joinMenuPath(parentPath, menu.path)
      const children = menu.children?.length ? mapRouterMenus(menu.children, fullPath) : undefined
      const hasChildren = Boolean(children?.length)
      const titleKey = menu.meta.i18nKey || resolveMenuTitleKey(fullPath)

      return {
        id: String(menu.id),
        index: hasChildren ? `menu-${menu.id}` : fullPath,
        path: hasChildren ? undefined : fullPath,
        titleKey,
        title: menu.meta.title,
        icon: menu.meta.icon,
        children
      }
    })
}

export function resolveMenuIcon(icon?: string): Component {
  if (!icon) {
    return Setting
  }
  const resolved = (ElementPlusIconsVue as Record<string, Component>)[icon]
  return resolved || Setting
}

export function collectMenuPaths(items: SidebarMenuItem[]): string[] {
  const paths: string[] = []
  for (const item of items) {
    if (item.path) {
      paths.push(item.path)
    }
    if (item.children?.length) {
      paths.push(...collectMenuPaths(item.children))
    }
  }
  return paths
}

export function filterMenuTree(
  items: SidebarMenuItem[],
  keyword: string,
  resolveLabel: (item: SidebarMenuItem) => string
): SidebarMenuItem[] {
  const normalized = keyword.trim().toLowerCase()
  if (!normalized) {
    return items
  }

  return items
    .map((item) => {
      if (item.children?.length) {
        const children = filterMenuTree(item.children, normalized, resolveLabel)
        if (children.length > 0) {
          return { ...item, children }
        }
      }
      if (resolveLabel(item).toLowerCase().includes(normalized)) {
        return item
      }
      return null
    })
    .filter((item): item is SidebarMenuItem => item !== null)
}

export function resolveMenuLabel(item: SidebarMenuItem, t: (key: string) => string) {
  if (item.titleKey) {
    const translated = t(item.titleKey)
    if (translated !== item.titleKey) {
      return translated
    }
  }
  return item.title || ''
}

/** 管理页展示：优先 i18n，缺失时回退数据库 MenuName */
export function resolveMenuDisplayName(
  menuName: string,
  i18nKey: string | undefined,
  path: string | undefined,
  t: (key: string) => string
): string {
  const titleKey = i18nKey || (path ? resolveMenuTitleKey(path) : undefined)
  if (titleKey) {
    const translated = t(titleKey)
    if (translated !== titleKey) {
      return translated
    }
  }
  return menuName
}
