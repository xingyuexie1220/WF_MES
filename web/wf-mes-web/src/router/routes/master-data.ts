import type { RouteRecordRaw } from 'vue-router'

export const masterDataRoutes: RouteRecordRaw[] = [
  {
    path: 'master-data/material',
    name: 'MasterMaterial',
    component: () => import('@/views/master-data/material/index.vue'),
    meta: { titleKey: 'menu.masterMaterial', permission: 'master:material:list', keepAlive: true }
  },
  {
    path: 'master-data/route',
    name: 'MasterRoute',
    component: () => import('@/views/master-data/route/index.vue'),
    meta: { titleKey: 'menu.masterRoute', permission: 'master:route:list', keepAlive: true }
  },
  {
    path: 'master-data/station',
    name: 'MasterStation',
    component: () => import('@/views/master-data/station/index.vue'),
    meta: { titleKey: 'menu.masterStation', permission: 'master:station:list', keepAlive: true }
  },
  {
    path: 'master-data/workcenter',
    name: 'MasterWorkCenter',
    component: () => import('@/views/master-data/workcenter/index.vue'),
    meta: { titleKey: 'menu.masterWorkCenter', permission: 'master:workcenter:list', keepAlive: true }
  }
]
