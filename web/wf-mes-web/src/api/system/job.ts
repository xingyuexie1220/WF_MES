import { http } from '@/utils/request/http'
import type { JobItem } from '@/types/system/job'

export const getJobsApi = () => http.get<JobItem[]>('/system/jobs')

export const runJobApi = (jobKey: string) => http.post<void>(`/system/jobs/run/${jobKey}`)
