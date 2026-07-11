export interface JobItem {
  jobKey: string
  jobName: string
  jobGroup: string
  cronExpression: string
  description: string
  status: string
  nextFireTime?: string
  previousFireTime?: string
}
