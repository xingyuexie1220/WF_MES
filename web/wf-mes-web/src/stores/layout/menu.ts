import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import type { RouterMenu } from '@/types/auth/user'
import { buildFallbackMenusWhenApiEmpty } from '@/config/systemMenuFallback'
import { mapRouterMenus, type SidebarMenuItem } from '@/utils/menu'
import { useUserStore } from '@/stores/auth/user'

function collectOpenKeys(items: SidebarMenuItem[]): string[] {
  const keys: string[] = []
  for (const item of items) {
    if (item.children?.length) {
      keys.push(item.index)
      keys.push(...collectOpenKeys(item.children))
    }
  }
  return keys
}

export function findMenuTitleByPath(items: SidebarMenuItem[], path: string): string | undefined {
  for (const item of items) {
    if (item.path === path) {
      if (item.titleKey) {
        return undefined
      }
      return item.title
    }
    if (item.children?.length) {
      const childTitle = findMenuTitleByPath(item.children, path)
      if (childTitle) {
        return childTitle
      }
    }
  }
  return undefined
}

export const useMenuStore = defineStore('wf-menu', () => {
  const keyword = ref('')
  const collapsed = ref(false)
  const loaded = ref(false)

  const homeMenu = computed(
    (): SidebarMenuItem => ({
      id: 'dashboard',
      index: '/dashboard',
      path: '/dashboard',
      titleKey: 'layout.home',
      icon: 'HomeFilled'
    })
  )

  const apiMenus = computed(() => {
    const userStore = useUserStore()
    return mapRouterMenus(userStore.routers)
  })

  const rawMenuTree = computed(() => {
    const menus = apiMenus.value
    const dynamicMenus = menus.length > 0 ? menus : buildFallbackMenusWhenApiEmpty()
    return [homeMenu.value, ...dynamicMenus]
  })

  const defaultOpeneds = computed(() => collectOpenKeys(rawMenuTree.value))

  function setKeyword(value: string) {
    keyword.value = value
  }

  function toggleCollapsed() {
    collapsed.value = !collapsed.value
  }

  async function loadMenus(force = false) {
    if (loaded.value && !force) {
      return
    }
    const userStore = useUserStore()
    try {
      await userStore.fetchRouters()
    } catch {
      userStore.routers = [] as RouterMenu[]
    } finally {
      loaded.value = true
    }
  }

  function reset() {
    keyword.value = ''
    collapsed.value = false
    loaded.value = false
  }

  return {
    keyword,
    collapsed,
    loaded,
    homeMenu,
    apiMenus,
    rawMenuTree,
    defaultOpeneds,
    setKeyword,
    toggleCollapsed,
    loadMenus,
    reset
  }
})
