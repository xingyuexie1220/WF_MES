import type { RouteRecordRaw } from 'vue-router'

export const barcodeRoutes: RouteRecordRaw[] = [
  {
    path: 'barcode/customer',
    name: 'BarcodeCustomer',
    component: () => import('@/views/barcode/customer/index.vue'),
    meta: {
      titleKey: 'route.barcodeCustomer',
      permission: 'barcode:customer:list',
      keepAlive: true
    }
  }
]
