import { ref } from 'vue'
import { defineStore } from 'pinia'
import {
  confirmPrintJobApi,
  createPrintJobApi,
  getCustomersApi,
  getMaterialRulesApi,
  saveCustomerApi
} from '@/api/barcode/customer'
import type {
  BarcodeCustomerDto,
  BarcodeMaterialRuleDto,
  CreatePrintJobRequest,
  PrintJobDto,
  SaveBarcodeCustomerRequest
} from '@/types/barcode/customer'
import { useLargeList } from '@/composables/useLargeList'

export const useBarcodeStore = defineStore('wf-barcode', () => {
  const customersLoadedAt = ref<number | null>(null)
  const materialRulesLoadedAt = ref<number | null>(null)
  const lastPrintJob = ref<PrintJobDto | null>(null)
  const { items: customers, loading: customersLoading, reset: resetCustomers, setLoading: setCustomersLoading } =
    useLargeList<BarcodeCustomerDto>()
  const {
    items: materialRules,
    loading: rulesLoading,
    reset: resetMaterialRules,
    setLoading: setRulesLoading
  } = useLargeList<BarcodeMaterialRuleDto>()

  async function fetchCustomers(force = false) {
    if (!force && customers.value.length > 0) {
      return customers.value
    }
    setCustomersLoading(true)
    try {
      const data = await getCustomersApi()
      resetCustomers(Array.isArray(data) ? data : [])
      customersLoadedAt.value = Date.now()
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

  async function fetchMaterialRules(force = false) {
    if (!force && materialRules.value.length > 0) {
      return materialRules.value
    }
    setRulesLoading(true)
    try {
      const data = await getMaterialRulesApi()
      resetMaterialRules(Array.isArray(data) ? data : [])
      materialRulesLoadedAt.value = Date.now()
      return materialRules.value
    } finally {
      setRulesLoading(false)
    }
  }

  async function createPrintJob(payload: CreatePrintJobRequest) {
    lastPrintJob.value = await createPrintJobApi(payload)
    return lastPrintJob.value
  }

  async function confirmPrintJob(jobId: string) {
    await confirmPrintJobApi(jobId)
  }

  function clearCache() {
    resetCustomers([])
    resetMaterialRules([])
    customersLoadedAt.value = null
    materialRulesLoadedAt.value = null
    lastPrintJob.value = null
  }

  return {
    customers,
    customersLoading,
    customersLoadedAt,
    materialRules,
    rulesLoading,
    materialRulesLoadedAt,
    lastPrintJob,
    fetchCustomers,
    saveCustomer,
    fetchMaterialRules,
    createPrintJob,
    confirmPrintJob,
    clearCache
  }
})
