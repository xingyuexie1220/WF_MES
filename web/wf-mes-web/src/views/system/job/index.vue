<script setup lang="ts">
defineOptions({ name: 'SystemJob' })
import { onMounted, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Refresh, Timer, VideoPlay } from '@element-plus/icons-vue'
import { getJobsApi, runJobApi } from '@/api/system/job'
import { WfPage, WfPageBody, WfTable } from '@/components/page'
import type { JobItem } from '@/types/system/job'

const { t } = useI18n()
const loading = ref(false)
const runningKey = ref('')
const tableData = ref<JobItem[]>([])

function formatDate(value?: string) {
  if (!value) return t('common.none')
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return value
  return date.toLocaleString()
}

async function loadData() {
  loading.value = true
  try {
    tableData.value = await getJobsApi()
  } finally {
    loading.value = false
  }
}

async function handleRun(row: JobItem) {
  await ElMessageBox.confirm(t('system.job.confirmRun', { name: row.jobName }), t('common.confirm'))
  runningKey.value = row.jobKey
  try {
    await runJobApi(row.jobKey)
    ElMessage.success(t('system.job.runSuccess'))
    await loadData()
  } finally {
    runningKey.value = ''
  }
}

onMounted(loadData)
</script>

<template>
  <WfPage class="wf-job-page">
    <WfPageBody class="wf-job-page__body">
      <div class="wf-list-panel">
        <div class="wf-list-panel__head">
          <div class="wf-list-panel__title-block">
            <div class="wf-list-panel__title">
              <el-icon class="wf-list-panel__title-icon"><Timer /></el-icon>
              {{ t('system.job.title') }}
              <span class="wf-list-panel__count">{{ t('system.job.jobTotal', { count: tableData.length }) }}</span>
            </div>
            <p class="wf-list-panel__desc">{{ t('system.job.pageDesc') }}</p>
          </div>
          <div class="wf-list-panel__toolbar">
            <div class="wf-list-panel__group wf-list-panel__group--action">
              <el-button :icon="Refresh" :loading="loading" @click="loadData">{{ t('common.refresh') }}</el-button>
            </div>
          </div>
        </div>

        <WfTable v-loading="loading" :data="tableData" class="wf-list-panel__table">
          <el-table-column prop="jobName" :label="t('system.job.colName')" min-width="140" />
          <el-table-column prop="jobKey" :label="t('system.job.colKey')" min-width="160" />
          <el-table-column prop="cronExpression" :label="t('system.job.colCron')" min-width="140" />
          <el-table-column prop="description" :label="t('system.job.colDesc')" min-width="260" show-overflow-tooltip />
          <el-table-column prop="nextFireTime" :label="t('system.job.colNext')" width="170">
            <template #default="{ row }">{{ formatDate(row.nextFireTime) }}</template>
          </el-table-column>
          <el-table-column prop="previousFireTime" :label="t('system.job.colPrev')" width="170">
            <template #default="{ row }">{{ formatDate(row.previousFireTime) }}</template>
          </el-table-column>
          <el-table-column :label="t('common.actions')" width="120" fixed="right" align="center">
            <template #default="{ row }">
              <el-button link type="primary" :loading="runningKey === row.jobKey" :icon="VideoPlay" @click="handleRun(row)">
                {{ t('system.job.run') }}
              </el-button>
            </template>
          </el-table-column>
        </WfTable>
      </div>
    </WfPageBody>
  </WfPage>
</template>

<style scoped lang="scss">
.wf-job-page {
  &__body {
    :deep(.el-card__body) {
      padding: 0;
      display: flex;
      flex-direction: column;
    }
  }
}
</style>
