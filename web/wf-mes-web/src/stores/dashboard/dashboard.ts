import { ref } from 'vue'
import { defineStore } from 'pinia'
import { getBigScreenOverviewApi, getReportOverviewApi } from '@/api/dashboard/index'
import type { DashboardOverview, ReportOverview } from '@/types/dashboard/overview'

export const useDashboardStore = defineStore('wf-dashboard', () => {
  const bigScreenOverview = ref<DashboardOverview | null>(null)
  const reportOverview = ref<ReportOverview | null>(null)
  const bigScreenLoadedAt = ref<number | null>(null)
  const reportLoadedAt = ref<number | null>(null)
  const loading = ref(false)

  async function fetchBigScreenOverview(force = false) {
    if (!force && bigScreenOverview.value) {
      return bigScreenOverview.value
    }
    loading.value = true
    try {
      bigScreenOverview.value = await getBigScreenOverviewApi()
      bigScreenLoadedAt.value = Date.now()
      return bigScreenOverview.value
    } finally {
      loading.value = false
    }
  }

  async function fetchReportOverview(force = false) {
    if (!force && reportOverview.value) {
      return reportOverview.value
    }
    loading.value = true
    try {
      reportOverview.value = await getReportOverviewApi()
      reportLoadedAt.value = Date.now()
      return reportOverview.value
    } finally {
      loading.value = false
    }
  }

  function clearCache() {
    bigScreenOverview.value = null
    reportOverview.value = null
    bigScreenLoadedAt.value = null
    reportLoadedAt.value = null
  }

  return {
    bigScreenOverview,
    reportOverview,
    bigScreenLoadedAt,
    reportLoadedAt,
    loading,
    fetchBigScreenOverview,
    fetchReportOverview,
    clearCache
  }
})
