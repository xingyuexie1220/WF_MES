import { http } from '@/utils/request/http'
import type { DashboardOverview, ReportOverview } from '@/types/dashboard/overview'

export const getBigScreenOverviewApi = () => http.get<DashboardOverview>('/dashboard/bigscreen/overview')

export const getReportOverviewApi = () => http.get<ReportOverview>('/dashboard/reports/overview')
