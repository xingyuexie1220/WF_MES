import type { RouteRecordRaw } from 'vue-router'

const developing = () => import('@/views/common/developing/index.vue')

export const masterDataRoutes: RouteRecordRaw[] = [
  {
    path: 'master-data/material',
    name: 'MasterMaterial',
    component: developing,
    meta: { titleKey: 'menu.masterMaterial', permission: 'master:material:list' }
  },
  {
    path: 'master-data/route',
    name: 'MasterRoute',
    component: developing,
    meta: { titleKey: 'menu.masterRoute', permission: 'master:route:list' }
  },
  {
    path: 'master-data/station',
    name: 'MasterStation',
    component: developing,
    meta: { titleKey: 'menu.masterStation', permission: 'master:station:list' }
  },
  {
    path: 'master-data/workcenter',
    name: 'MasterWorkCenter',
    component: developing,
    meta: { titleKey: 'menu.masterWorkCenter', permission: 'master:workcenter:list' }
  }
]
