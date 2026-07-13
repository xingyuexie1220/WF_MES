import { defineStore } from 'pinia'
import { getCustomersApi, saveCustomerApi } from '@/api/barcode/customer'
import type { BarcodeCustomerDto, SaveBarcodeCustomerRequest } from '@/types/barcode/customer'
import { useLargeList } from '@/composables/useLargeList'

export const useBarcodeStore = defineStore('wf-barcode', () => {
  const {
    items: customers,
    loading: customersLoading,
    reset: resetCustomers,
    setLoading: setCustomersLoading
  } = useLargeList<BarcodeCustomerDto>()

  async function fetchCustomers(force = false) {
    if (!force && customers.value.length > 0) {
      return customers.value
    }
    setCustomersLoading(true)
    try {
      const data = await getCustomersApi()
      resetCustomers(Array.isArray(data) ? data : [])
      return customers.value
    } finally {
      setCustomersLoading(false)
    }
  }

  async function saveCustomer(payload: SaveBarcodeCustomerRequest) {
    const id = await saveCustomerApi(payload)
    await fetchCustomers(true)
    return id
  }

  function clearCache() {
    resetCustomers([])
  }

  return {
    customers,
    customersLoading,
    fetchCustomers,
    saveCustomer,
    clearCache
  }
})
