import { ref } from 'vue'
import { defineStore } from 'pinia'
import router from '@/router'

export interface WfTabItem {
  path: string
  titleKey: string
  tabTitleKey?: string
  title?: string
  closable?: boolean
  routeName?: string
  keepAlive?: boolean
}

const DEFAULT_TABS: WfTabItem[] = [
  {
    path: '/dashboard',
    titleKey: 'route.dashboard',
    tabTitleKey: 'layout.home',
    closable: false,
    routeName: 'Dashboard',
    keepAlive: true
  }
]

export const useTabsStore = defineStore('wf-tabs', () => {
  const tabs = ref<WfTabItem[]>([...DEFAULT_TABS])
  const activePath = ref('/dashboard')
  const refreshKeys = ref<Record<string, number>>({})
  const cachedRouteNames = ref<string[]>(['Dashboard'])

  function syncCachedRouteNames() {
    cachedRouteNames.value = tabs.value
      .filter((tab) => tab.keepAlive && tab.routeName)
      .map((tab) => tab.routeName as string)
  }

  function addTab(
    path: string,
    titleKey: string,
    closable = true,
    title?: string,
    tabTitleKey?: string,
    routeName?: string,
    keepAlive = false
  ) {
    const existing = tabs.value.find((tab) => tab.path === path)
    if (!existing) {
      tabs.value.push({ path, titleKey, tabTitleKey, closable, title, routeName, keepAlive })
    } else {
      if (title) {
        existing.title = title
      }
      if (tabTitleKey) {
        existing.tabTitleKey = tabTitleKey
      }
      if (routeName) {
        existing.routeName = routeName
      }
      existing.keepAlive = keepAlive
    }
    activePath.value = path
    syncCachedRouteNames()
  }

  function removeTab(path: string) {
    const tab = tabs.value.find((item) => item.path === path)
    if (!tab || tab.closable === false) {
      return
    }
    const index = tabs.value.findIndex((item) => item.path === path)
    if (index === -1) {
      return
    }
    tabs.value.splice(index, 1)
    delete refreshKeys.value[path]
    syncCachedRouteNames()
    if (activePath.value === path) {
      const next = tabs.value[Math.max(index - 1, 0)]
      activePath.value = next.path
      router.push(next.path)
    }
  }

  function setActive(path: string) {
    activePath.value = path
    if (router.currentRoute.value.path !== path) {
      router.push(path)
    }
  }

  function closeLeft(path: string) {
    const index = tabs.value.findIndex((tab) => tab.path === path)
    if (index <= 0) {
      return
    }
    const removed = tabs.value.splice(1, index - 1)
    removed.forEach((tab) => delete refreshKeys.value[tab.path])
    syncCachedRouteNames()
    if (!tabs.value.some((tab) => tab.path === activePath.value)) {
      setActive(path)
    }
  }

  function closeRight(path: string) {
    const index = tabs.value.findIndex((tab) => tab.path === path)
    if (index === -1) {
      return
    }
    const removed = tabs.value.splice(index + 1)
    removed.forEach((tab) => delete refreshKeys.value[tab.path])
    syncCachedRouteNames()
    if (!tabs.value.some((tab) => tab.path === activePath.value)) {
      setActive(path)
    }
  }

  function closeOthers(path: string) {
    const current = tabs.value.find((tab) => tab.path === path)
    if (!current) {
      return
    }
    const keep = tabs.value.filter((tab) => tab.closable === false || tab.path === path)
    tabs.value.filter((tab) => !keep.includes(tab)).forEach((tab) => delete refreshKeys.value[tab.path])
    tabs.value = keep
    syncCachedRouteNames()
    setActive(path)
  }

  function closeAll() {
    const dashboard = tabs.value.find((tab) => tab.closable === false)
    tabs.value = dashboard ? [dashboard] : []
    refreshKeys.value = {}
    syncCachedRouteNames()
    const target = dashboard?.path || '/dashboard'
    setActive(target)
  }

  function refreshCurrentView(path?: string) {
    const target = path || activePath.value
    refreshKeys.value[target] = (refreshKeys.value[target] || 0) + 1
  }

  function getTabLabel(tab: WfTabItem, t: (key: string) => string) {
    if (tab.tabTitleKey) {
      return t(tab.tabTitleKey)
    }
    if (tab.titleKey) {
      return t(tab.titleKey)
    }
    if (tab.title) {
      return tab.title
    }
    return tab.path
  }

  function reset() {
    tabs.value = [...DEFAULT_TABS]
    activePath.value = '/dashboard'
    refreshKeys.value = {}
    cachedRouteNames.value = ['Dashboard']
  }

  return {
    tabs,
    activePath,
    refreshKeys,
    cachedRouteNames,
    addTab,
    removeTab,
    setActive,
    closeLeft,
    closeRight,
    closeOthers,
    closeAll,
    refreshCurrentView,
    getTabLabel,
    reset
  }
})
