import type { RouteRecordRaw } from 'vue-router'

const developing = () => import('@/views/common/developing/index.vue')

export const reportRoutes: RouteRecordRaw[] = [
  {
    path: 'report/output',
    name: 'ReportOutput',
    component: developing,
    meta: { titleKey: 'menu.reportOutput', permission: 'dashboard:report:view' }
  },
  {
    path: 'report/wip',
    name: 'ReportWip',
    component: developing,
    meta: { titleKey: 'menu.reportWip', permission: 'dashboard:report:view' }
  }
]
