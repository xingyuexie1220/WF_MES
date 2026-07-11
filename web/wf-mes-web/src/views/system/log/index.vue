<script setup lang="ts">
defineOptions({ name: 'SystemLog' })
import { computed, onMounted, reactive, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Delete, Document, Download, Search } from '@element-plus/icons-vue'
import {
  clearExceptionLogsApi,
  clearOperationLogsApi,
  exportExceptionLogsApi,
  exportOperationLogsApi,
  getExceptionLogPageApi,
  getOperationLogPageApi
} from '@/api/system/log'
import { WfPage, WfPageBody, WfPagePager, WfTable } from '@/components/page'
import { exportExcel } from '@/utils/exportExcel'
import type { ExceptionLogItem, OperationLogItem } from '@/types/system/log'

const { t } = useI18n()

const activeTab = ref<'operation' | 'exception'>('operation')
const operLoading = ref(false)
const exceptionLoading = ref(false)
const operList = ref<OperationLogItem[]>([])
const exceptionList = ref<ExceptionLogItem[]>([])
const operTotal = ref(0)
const exceptionTotal = ref(0)

const operQuery = reactive({ pageIndex: 1, pageSize: 20, module: '', operUserName: '' })
const exceptionQuery = reactive({ pageIndex: 1, pageSize: 20, module: '', operUserName: '' })

const currentTotal = computed(() =>
  activeTab.value === 'operation' ? operTotal.value : exceptionTotal.value
)

function formatDate(value?: string) {
  if (!value) return t('common.none')
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return value
  return date.toLocaleString()
}

async function loadOper() {
  operLoading.value = true
  try {
    const result = await getOperationLogPageApi({ ...operQuery })
    operList.value = result.items
    operTotal.value = result.total
  } finally {
    operLoading.value = false
  }
}

async function loadException() {
  exceptionLoading.value = true
  try {
    const result = await getExceptionLogPageApi({ ...exceptionQuery })
    exceptionList.value = result.items
    exceptionTotal.value = result.total
  } finally {
    exceptionLoading.value = false
  }
}

function handleTabChange(name: string | number) {
  if (name === 'operation') void loadOper()
  else void loadException()
}

function handleOperSearch() {
  operQuery.pageIndex = 1
  void loadOper()
}

function handleExceptionSearch() {
  exceptionQuery.pageIndex = 1
  void loadException()
}

function resetOperSearch() {
  operQuery.module = ''
  operQuery.operUserName = ''
  operQuery.pageIndex = 1
  void loadOper()
}

function resetExceptionSearch() {
  exceptionQuery.module = ''
  exceptionQuery.operUserName = ''
  exceptionQuery.pageIndex = 1
  void loadException()
}

async function handleClear() {
  const name =
    activeTab.value === 'operation' ? t('system.log.operationTab') : t('system.log.exceptionTab')
  await ElMessageBox.confirm(t('system.log.confirmClear', { name }), t('common.tip'), { type: 'warning' })
  if (activeTab.value === 'operation') {
    await clearOperationLogsApi()
    await loadOper()
  } else {
    await clearExceptionLogsApi()
    await loadException()
  }
  ElMessage.success(t('system.log.clearSuccess'))
}

async function handleExport() {
  const timestamp = new Date().toISOString().slice(0, 10).replace(/-/g, '')
  if (activeTab.value === 'operation') {
    const rows = await exportOperationLogsApi({ ...operQuery, pageIndex: 1, pageSize: 10000 })
    if (!rows.length) {
      ElMessage.warning(t('system.log.exportEmpty'))
      return
    }
    exportExcel(
      `${t('system.log.operationExportFile')}_${timestamp}.xlsx`,
      t('system.log.operationTab'),
      [
        { header: t('system.log.colModule'), key: 'module', width: 14 },
        { header: t('system.log.colTitle'), key: 'title', width: 14 },
        { header: t('system.log.colBusinessType'), key: 'businessType', width: 12 },
        { header: t('system.log.colMethod'), key: 'requestMethod', width: 10 },
        { header: t('system.log.colUrl'), key: 'operUrl', width: 28 },
        { header: t('system.log.filterUser'), key: 'operUserName', width: 12 },
        { header: t('system.log.colIp'), key: 'operIp', width: 14 },
        { header: t('system.log.colCost'), key: 'costTime', width: 10 },
        { header: t('system.log.colTime'), key: 'operTime', width: 20 }
      ],
      rows
    )
  } else {
    const rows = await exportExceptionLogsApi({ ...exceptionQuery, pageIndex: 1, pageSize: 10000 })
    if (!rows.length) {
      ElMessage.warning(t('system.log.exportEmpty'))
      return
    }
    exportExcel(
      `${t('system.log.exceptionExportFile')}_${timestamp}.xlsx`,
      t('system.log.exceptionTab'),
      [
        { header: t('system.log.colModule'), key: 'module', width: 14 },
        { header: t('system.log.colMessage'), key: 'message', width: 36 },
        { header: t('system.log.colMethod'), key: 'requestMethod', width: 10 },
        { header: t('system.log.colUrl'), key: 'requestUrl', width: 28 },
        { header: t('system.log.filterUser'), key: 'operUserName', width: 12 },
        { header: t('system.log.colIp'), key: 'operIp', width: 14 },
        { header: t('system.log.colTime'), key: 'exceptionTime', width: 20 }
      ],
      rows
    )
  }
}

onMounted(() => loadOper())
</script>

<template>
  <WfPage>
    <WfPageBody class="wf-log-page__body-wrap">
      <div class="wf-log-page">
        <header class="wf-log-page__header">
          <div class="wf-log-page__header-main">
            <span class="wf-log-page__header-icon">
              <el-icon><Document /></el-icon>
            </span>
            <div class="wf-log-page__header-text">
              <h2 class="wf-log-page__header-title">{{ t('system.log.title') }}</h2>
              <p class="wf-log-page__header-desc">{{ t('system.log.pageDesc') }}</p>
            </div>
          </div>
          <div class="wf-log-page__header-actions">
            <span class="wf-log-page__header-badge">{{ t('system.log.logTotal', { count: currentTotal }) }}</span>
            <el-button :icon="Download" @click="handleExport">{{ t('common.export') }}</el-button>
            <el-button plain type="danger" :icon="Delete" @click="handleClear">{{ t('system.log.clear') }}</el-button>
          </div>
        </header>

        <section class="wf-log-page__panel">
          <el-tabs v-model="activeTab" class="wf-log-page__tabs" @tab-change="handleTabChange">
            <el-tab-pane :label="t('system.log.operationTab')" name="operation">
              <div class="wf-log-page__panel-toolbar">
                <div class="wf-log-page__panel-filters">
                  <el-input
                    v-model="operQuery.module"
                    clearable
                    :placeholder="t('system.log.filterModule')"
                    class="wf-log-page__filter-input"
                    @keyup.enter="handleOperSearch"
                  />
                  <el-input
                    v-model="operQuery.operUserName"
                    clearable
                    :placeholder="t('system.log.filterUser')"
                    class="wf-log-page__filter-input"
                    @keyup.enter="handleOperSearch"
                  />
                </div>
                <div class="wf-log-page__panel-actions">
                  <el-button type="primary" :icon="Search" @click="handleOperSearch">{{ t('common.query') }}</el-button>
                  <el-button @click="resetOperSearch">{{ t('common.reset') }}</el-button>
                </div>
              </div>

              <WfTable v-loading="operLoading" :data="operList" class="wf-log-page__table">
                <el-table-column prop="module" :label="t('system.log.colModule')" width="120" show-overflow-tooltip />
                <el-table-column prop="title" :label="t('system.log.colTitle')" width="140" show-overflow-tooltip />
                <el-table-column prop="businessType" :label="t('system.log.colBusinessType')" width="90" />
                <el-table-column prop="method" :label="t('system.log.colMethod')" min-width="200" show-overflow-tooltip />
                <el-table-column prop="operUrl" :label="t('system.log.colUrl')" min-width="180" show-overflow-tooltip />
                <el-table-column prop="operUserName" :label="t('system.log.filterUser')" width="100" />
                <el-table-column prop="operIp" :label="t('system.log.colIp')" width="130" />
                <el-table-column prop="costTime" :label="t('system.log.colCost')" width="88" align="right">
                  <template #default="{ row }">
                    <span class="wf-log-page__cost">{{ row.costTime }} ms</span>
                  </template>
                </el-table-column>
                <el-table-column prop="operTime" :label="t('system.log.colTime')" width="170">
                  <template #default="{ row }">{{ formatDate(row.operTime) }}</template>
                </el-table-column>
                <template #empty>
                  <el-empty :description="t('system.log.emptyData')" :image-size="72" />
                </template>
              </WfTable>

              <WfPagePager
                v-model:current-page="operQuery.pageIndex"
                v-model:page-size="operQuery.pageSize"
                :total="operTotal"
                class="wf-log-page__pager"
                @change="loadOper"
              />
            </el-tab-pane>

            <el-tab-pane :label="t('system.log.exceptionTab')" name="exception">
              <div class="wf-log-page__panel-toolbar">
                <div class="wf-log-page__panel-filters">
                  <el-input
                    v-model="exceptionQuery.module"
                    clearable
                    :placeholder="t('system.log.filterModule')"
                    class="wf-log-page__filter-input"
                    @keyup.enter="handleExceptionSearch"
                  />
                  <el-input
                    v-model="exceptionQuery.operUserName"
                    clearable
                    :placeholder="t('system.log.filterUser')"
                    class="wf-log-page__filter-input"
                    @keyup.enter="handleExceptionSearch"
                  />
                </div>
                <div class="wf-log-page__panel-actions">
                  <el-button type="primary" :icon="Search" @click="handleExceptionSearch">{{ t('common.query') }}</el-button>
                  <el-button @click="resetExceptionSearch">{{ t('common.reset') }}</el-button>
                </div>
              </div>

              <WfTable v-loading="exceptionLoading" :data="exceptionList" class="wf-log-page__table">
                <el-table-column prop="module" :label="t('system.log.colModule')" width="120" show-overflow-tooltip />
                <el-table-column prop="message" :label="t('system.log.colMessage')" min-width="220" show-overflow-tooltip />
                <el-table-column prop="requestMethod" :label="t('system.log.colMethod')" width="90">
                  <template #default="{ row }">
                    <code v-if="row.requestMethod" class="wf-log-page__code">{{ row.requestMethod }}</code>
                    <span v-else>{{ t('common.none') }}</span>
                  </template>
                </el-table-column>
                <el-table-column prop="requestUrl" :label="t('system.log.colUrl')" min-width="180" show-overflow-tooltip />
                <el-table-column prop="operUserName" :label="t('system.log.filterUser')" width="100" />
                <el-table-column prop="operIp" :label="t('system.log.colIp')" width="130" />
                <el-table-column prop="exceptionTime" :label="t('system.log.colTime')" width="170">
                  <template #default="{ row }">{{ formatDate(row.exceptionTime) }}</template>
                </el-table-column>
                <template #empty>
                  <el-empty :description="t('system.log.emptyData')" :image-size="72" />
                </template>
              </WfTable>

              <WfPagePager
                v-model:current-page="exceptionQuery.pageIndex"
                v-model:page-size="exceptionQuery.pageSize"
                :total="exceptionTotal"
                class="wf-log-page__pager"
                @change="loadException"
              />
            </el-tab-pane>
          </el-tabs>
        </section>
      </div>
    </WfPageBody>
  </WfPage>
</template>

<style scoped lang="scss">
.wf-log-page {
  &__body-wrap {
    :deep(.el-card__body) {
      padding: 0;
      display: flex;
      flex-direction: column;
      min-height: 0;
      background: var(--wf-page-bg);
    }
  }

  flex: 1;
  min-height: 0;
  display: flex;
  flex-direction: column;
  padding: 12px;
  gap: 12px;

  &__header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 16px;
    padding: 14px 16px;
    background: #fff;
    border: 1px solid #e8eaed;
    border-radius: 8px;
    box-shadow: var(--wf-card-shadow);
    flex-shrink: 0;
    flex-wrap: wrap;
  }

  &__header-main {
    display: flex;
    align-items: center;
    gap: 12px;
    min-width: 0;
  }

  &__header-icon {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 40px;
    height: 40px;
    border-radius: 10px;
    background: linear-gradient(135deg, #edf0ff 0%, #e8f3ff 100%);
    color: var(--wf-tab-primary);
    font-size: 20px;
    flex-shrink: 0;
  }

  &__header-text {
    min-width: 0;
  }

  &__header-title {
    margin: 0;
    font-size: 16px;
    font-weight: 600;
    color: var(--wf-text);
    line-height: 1.4;
  }

  &__header-desc {
    margin: 4px 0 0;
    font-size: 12px;
    color: var(--wf-text-secondary);
    line-height: 1.5;
  }

  &__header-actions {
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    gap: 10px;
    flex-shrink: 0;
  }

  &__header-badge {
    padding: 4px 12px;
    font-size: 12px;
    font-weight: 600;
    color: var(--wf-tab-primary);
    background: var(--wf-tab-active-bg);
    border-radius: 999px;
    white-space: nowrap;
  }

  &__panel {
    flex: 1;
    min-height: 0;
    display: flex;
    flex-direction: column;
    padding: 0 14px 12px;
    background: #fff;
    border: 1px solid #e8eaed;
    border-radius: 8px;
    box-shadow: var(--wf-card-shadow);
  }

  &__tabs {
    flex: 1;
    min-height: 0;
    display: flex;
    flex-direction: column;

    :deep(.el-tabs__header) {
      margin: 0;
      padding-top: 4px;
      border-bottom: 1px solid #eef0f3;
    }

    :deep(.el-tabs__nav-wrap::after) {
      display: none;
    }

    :deep(.el-tabs__item) {
      height: 44px;
      font-size: 14px;
      font-weight: 500;
      color: var(--wf-text-secondary);

      &.is-active {
        color: var(--wf-tab-primary);
        font-weight: 600;
      }
    }

    :deep(.el-tabs__active-bar) {
      height: 3px;
      border-radius: 2px 2px 0 0;
      background: var(--wf-tab-primary);
    }

    :deep(.el-tabs__content) {
      flex: 1;
      min-height: 0;
    }

    :deep(.el-tab-pane) {
      height: 100%;
      display: flex;
      flex-direction: column;
      min-height: 0;
      padding-top: 12px;
    }
  }

  &__panel-toolbar {
    display: grid;
    grid-template-columns: minmax(0, 1fr) auto;
    gap: 12px;
    align-items: center;
    min-height: 44px;
    margin-bottom: 10px;
    flex-shrink: 0;
  }

  &__panel-filters {
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    gap: 8px;
    min-width: 0;
  }

  &__panel-actions {
    display: flex;
    align-items: center;
    justify-content: flex-end;
    flex-wrap: wrap;
    gap: 8px;
  }

  &__filter-input {
    width: 160px;
  }

  &__table {
    flex: 1;
    min-height: 0;
  }

  &__pager {
    flex-shrink: 0;
    display: flex;
    justify-content: flex-end;
    margin-top: 10px;
    padding-top: 8px;
    border-top: 1px solid #f0f2f5;
  }

  &__cost {
    font-variant-numeric: tabular-nums;
    color: var(--wf-text-secondary);
    font-size: 12px;
  }

  &__code {
    display: inline-block;
    padding: 2px 8px;
    font-family: ui-monospace, Consolas, monospace;
    font-size: 12px;
    color: #4e5969;
    background: #f2f3f5;
    border: 1px solid #e8eaed;
    border-radius: 4px;
  }
}

@media (max-width: 900px) {
  .wf-log-page {
    &__panel-toolbar {
      grid-template-columns: 1fr;
    }

    &__panel-actions {
      justify-content: flex-start;
    }

    &__header-actions {
      width: 100%;
      justify-content: flex-end;
    }
  }
}
</style>
