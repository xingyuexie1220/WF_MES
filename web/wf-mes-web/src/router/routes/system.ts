import type { RouteRecordRaw } from 'vue-router'

export const systemRoutes: RouteRecordRaw[] = [
  {
    path: 'system/dept',
    name: 'SystemDept',
    component: () => import('@/views/system/dept/index.vue'),
    meta: { titleKey: 'route.systemOrgStructure', permission: 'system:dept:list', keepAlive: true }
  },
  {
    path: 'system/user',
    name: 'SystemUser',
    component: () => import('@/views/system/user/index.vue'),
    meta: { titleKey: 'route.systemUser', permission: 'system:user:list', keepAlive: true }
  },
  {
    path: 'system/menu',
    name: 'SystemMenu',
    component: () => import('@/views/system/menu/index.vue'),
    meta: { titleKey: 'route.systemMenu', permission: 'system:menu:list', keepAlive: true }
  },
  {
    path: 'system/role',
    name: 'SystemRole',
    component: () => import('@/views/system/role/index.vue'),
    meta: { titleKey: 'route.systemRole', permission: 'system:role:list', keepAlive: true }
  },
  {
    path: 'system/dict',
    name: 'SystemDict',
    component: () => import('@/views/system/dict/index.vue'),
    meta: { titleKey: 'route.systemDict', permission: 'system:dict:list', keepAlive: true }
  },
  {
    path: 'system/notice',
    name: 'SystemNotice',
    component: () => import('@/views/system/notice/index.vue'),
    meta: { titleKey: 'route.systemNotice', permission: 'system:notice:list', keepAlive: true }
  },
  {
    path: 'system/job',
    name: 'SystemJob',
    component: () => import('@/views/system/job/index.vue'),
    meta: { titleKey: 'route.systemJob', permission: 'system:job:list', keepAlive: true }
  },
  {
    path: 'system/log',
    name: 'SystemLog',
    component: () => import('@/views/system/log/index.vue'),
    meta: { titleKey: 'route.systemLog', permission: 'system:log:list', keepAlive: true }
  },
  {
    path: 'system/session',
    name: 'SystemSession',
    component: () => import('@/views/system/session/index.vue'),
    meta: { titleKey: 'route.systemSession', permission: 'system:session:list', keepAlive: true }
  },
  {
    path: 'system/factory',
    name: 'SystemFactory',
    component: () => import('@/views/system/factory/index.vue'),
    meta: { titleKey: 'route.systemFactory', permission: 'system:factory:list', keepAlive: true }
  },
  {
    path: 'system/position',
    name: 'SystemPosition',
    component: () => import('@/views/system/position/index.vue'),
    meta: { titleKey: 'route.systemPosition', permission: 'system:position:list', keepAlive: true }
  }
]
