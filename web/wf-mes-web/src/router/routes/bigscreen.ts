import type { RouteRecordRaw } from 'vue-router'

export const bigscreenRoute: RouteRecordRaw = {
  path: '/bigscreen',
  name: 'BigScreen',
  component: () => import('@/views/bigscreen/index.vue'),
  meta: {
    public: false,
    titleKey: 'menu.bigscreen',
    permission: 'dashboard:bigscreen:view',
    noTab: true,
    keepAlive: false
  }
}
