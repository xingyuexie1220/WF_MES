import { createRouter, createWebHistory } from 'vue-router'
import { authRoutes } from '@/router/routes/auth'
import { bigscreenRoute } from '@/router/routes/bigscreen'
import { masterDataRoutes } from '@/router/routes/master-data'
import { reportRoutes } from '@/router/routes/report'
import { systemRoutes } from '@/router/routes/system'
import { barcodeRoutes } from '@/router/routes/barcode'
import { setupRouterGuards } from '@/router/guards'

declare module 'vue-router' {
  interface RouteMeta {
    public?: boolean
    changePasswordOnly?: boolean
    titleKey?: string
    tabTitleKey?: string
    permission?: string
    keepAlive?: boolean
    noTab?: boolean
  }
}

const router = createRouter({
  history: createWebHistory(),
  routes: [
    ...authRoutes,
    bigscreenRoute,
    {
      path: '/',
      component: () => import('@/layout/WfLayout.vue'),
      redirect: '/dashboard',
      children: [
        {
          path: 'dashboard',
          name: 'Dashboard',
          component: () => import('@/views/dashboard/index.vue'),
          meta: { titleKey: 'route.dashboard', tabTitleKey: 'layout.home', keepAlive: true }
        },
        ...systemRoutes,
        ...masterDataRoutes,
        ...reportRoutes,
        ...barcodeRoutes
      ]
    }
  ]
})

setupRouterGuards(router)

export default router
