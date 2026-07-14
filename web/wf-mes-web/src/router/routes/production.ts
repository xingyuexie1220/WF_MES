import type { RouteRecordRaw } from 'vue-router'

export const productionRoutes: RouteRecordRaw[] = [
  {
    path: 'production/work-order',
    name: 'ProductionWorkOrder',
    component: () => import('@/views/production/work-order/index.vue'),
    meta: { titleKey: 'menu.productionWorkOrder', permission: 'mes:workorder:scan', keepAlive: true }
  }
]
