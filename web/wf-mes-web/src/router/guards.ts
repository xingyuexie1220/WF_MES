import type { Router } from 'vue-router'
import NProgress from 'nprogress'
import { useUserStore } from '@/stores/auth/user'
import { useMenuStore, findMenuTitleByPath } from '@/stores/layout/menu'
import { useTabsStore } from '@/stores/layout/tabs'

NProgress.configure({ showSpinner: false })

export function setupRouterGuards(router: Router) {
  router.beforeEach(async (to, _from, next) => {
    if (!to.meta.public) {
      NProgress.start()
    }

    const userStore = useUserStore()
    const menuStore = useMenuStore()

    if (to.meta.public) {
      next()
      return
    }

    if (!userStore.token) {
      next('/login')
      return
    }

    if (!userStore.userInfo) {
      await userStore.fetchUserInfo()
    }

    const mustChangePassword = !!userStore.userInfo?.mustChangePassword

    if (mustChangePassword && !to.meta.changePasswordOnly) {
      next('/change-password')
      return
    }

    if (!mustChangePassword && to.meta.changePasswordOnly) {
      next('/dashboard')
      return
    }

    if (to.meta.changePasswordOnly) {
      next()
      return
    }

    if (!menuStore.loaded) {
      await menuStore.loadMenus()
    }

    const permission = to.meta.permission
    if (permission && !userStore.hasPermission(permission)) {
      next('/dashboard')
      return
    }

    next()
  })

  router.afterEach((to) => {
    NProgress.done()

    if (to.meta.public || to.meta.noTab || !to.meta.titleKey) {
      return
    }

    const tabsStore = useTabsStore()
    const menuStore = useMenuStore()
    const dynamicTitle = findMenuTitleByPath(menuStore.rawMenuTree, to.path)
    const routeName = typeof to.name === 'string' ? to.name : undefined
    tabsStore.addTab(
      to.path,
      to.meta.titleKey,
      to.path !== '/dashboard',
      dynamicTitle,
      to.meta.tabTitleKey,
      routeName,
      !!to.meta.keepAlive
    )
  })
}
