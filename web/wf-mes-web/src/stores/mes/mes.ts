import { ref } from 'vue'
import { defineStore } from 'pinia'
import { getWorkOrdersApi, passStationApi } from '@/api/production/work-order'
import type { PassStationRequest, WorkOrderDto } from '@/types/production/work-order'
import { useLargeList } from '@/composables/useLargeList'

export const useMesStore = defineStore('wf-mes', () => {
  const lastFetchedAt = ref<number | null>(null)
  const { items: workOrders, loading, reset, setLoading } = useLargeList<WorkOrderDto>()

  async function fetchWorkOrders(force = false) {
    if (!force && workOrders.value.length > 0) {
      return workOrders.value
    }
    setLoading(true)
    try {
      const data = await getWorkOrdersApi()
      const list = Array.isArray(data) ? data : []
      reset(list)
      lastFetchedAt.value = Date.now()
      return workOrders.value
    } finally {
      setLoading(false)
    }
  }

  async function passStation(payload: PassStationRequest) {
    return passStationApi(payload)
  }

  function clearCache() {
    reset([])
    lastFetchedAt.value = null
  }

  return {
    workOrders,
    loading,
    lastFetchedAt,
    fetchWorkOrders,
    passStation,
    clearCache
  }
})
