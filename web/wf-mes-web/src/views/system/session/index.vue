<script setup lang="ts">
defineOptions({ name: 'SystemSession' })
import { computed, onMounted, reactive, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Connection, Refresh, Search, SwitchButton } from '@element-plus/icons-vue'
import { getSessionPageApi, kickSessionApi } from '@/api/system/session'
import { WfPage, WfPageBody, WfPagePager, WfTable } from '@/components/page'
import { useUserStore } from '@/stores/auth/user'
import type { SessionItem } from '@/types/system/session'

const { t } = useI18n()
const userStore = useUserStore()

const loading = ref(false)
const kickingKey = ref('')
const tableData = ref<SessionItem[]>([])
const total = ref(0)

const query = reactive({
  pageIndex: 1,
  pageSize: 20,
  userName: '',
  clientType: undefined as number | undefined,
  onlyActive: true
})

const canKick = computed(() => userStore.hasPermission('system:session:kick'))

const clientTypeOptions = computed(() => [
  { label: t('system.session.clientWeb'), value: 1 },
  { label: t('system.session.clientMobile'), value: 2 },
  { label: t('system.session.clientDesktop'), value: 3 }
])

const clientTypeTagType = computed(() => ({
  1: 'primary',
  2: 'warning',
  3: 'success'
} as Record<number, 'primary' | 'warning' | 'success'>))

function clientTypeLabel(value: number) {
  return clientTypeOptions.value.find((item) => item.value === value)?.label ?? String(value)
}

function clientTypeTag(value: number) {
  return clientTypeTagType.value[value] ?? 'info'
}

function formatDateShort(value?: string) {
  if (!value) return t('common.none')
  const date = new Date(value)
  if (Number.isNaN(date.getTime()) || date.getFullYear() < 1970) return t('common.none')
  const y = date.getFullYear()
  const m = String(date.getMonth() + 1).padStart(2, '0')
  const d = String(date.getDate()).padStart(2, '0')
  const hh = String(date.getHours()).padStart(2, '0')
  const mm = String(date.getMinutes()).padStart(2, '0')
  return `${y}-${m}-${d} ${hh}:${mm}`
}

function rowKey(row: SessionItem) {
  return `${row.userId}-${row.clientType}`
}

async function loadData() {
  loading.value = true
  try {
    const result = await getSessionPageApi({ ...query })
    tableData.value = result.items
    total.value = result.total
  } finally {
    loading.value = false
  }
}

function handleSearch() {
  query.pageIndex = 1
  void loadData()
}

function resetSearch() {
  query.userName = ''
  query.clientType = undefined
  query.onlyActive = true
  query.pageIndex = 1
  void loadData()
}

async function handleKick(row: SessionItem) {
  const name = row.nickName ? `${row.userName}（${row.nickName}）` : row.userName
  await ElMessageBox.confirm(
    t('system.session.confirmKick', { name, client: clientTypeLabel(row.clientType) }),
    t('common.confirm'),
    { type: 'warning' }
  )
  kickingKey.value = rowKey(row)
  try {
    await kickSessionApi(row.userId, row.clientType)
    ElMessage.success(t('system.session.kickSuccess'))
    await loadData()
  } finally {
    kickingKey.value = ''
  }
}

onMounted(loadData)
</script>

<template>
  <WfPage class="wf-session-page">
    <WfPageBody class="wf-session-page__body">
      <div class="wf-list-panel">
        <div class="wf-list-panel__head">
          <div class="wf-list-panel__title-block">
            <div class="wf-list-panel__title">
              <el-icon class="wf-list-panel__title-icon"><Connection /></el-icon>
              <span>{{ t('system.session.title') }}</span>
              <span class="wf-list-panel__count">{{ t('system.session.sessionTotal', { count: total }) }}</span>
            </div>
            <p class="wf-list-panel__desc">{{ t('system.session.pageDesc') }}</p>
          </div>
          <div class="wf-list-panel__toolbar">
            <div class="wf-list-panel__group wf-list-panel__group--filter">
              <el-input
                v-model="query.userName"
                clearable
                :prefix-icon="Search"
                :placeholder="t('system.session.filterUser')"
                class="wf-list-panel__input wf-list-panel__input--wide"
                @keyup.enter="handleSearch"
              />
              <el-select
                v-model="query.clientType"
                clearable
                :placeholder="t('system.session.filterClient')"
                class="wf-list-panel__select"
              >
                <el-option
                  v-for="item in clientTypeOptions"
                  :key="item.value"
                  :label="item.label"
                  :value="item.value"
                />
              </el-select>
              <el-checkbox v-model="query.onlyActive" class="wf-session-page__only-active">
                {{ t('system.session.onlyActive') }}
              </el-checkbox>
            </div>
            <span class="wf-list-panel__divider" aria-hidden="true" />
            <div class="wf-list-panel__group wf-list-panel__group--query">
              <el-button type="primary" :icon="Search" @click="handleSearch">{{ t('common.query') }}</el-button>
            </div>
            <span class="wf-list-panel__divider" aria-hidden="true" />
            <div class="wf-list-panel__group wf-list-panel__group--action">
              <el-button @click="resetSearch">{{ t('common.reset') }}</el-button>
              <el-button :icon="Refresh" :loading="loading" @click="loadData">{{ t('common.refresh') }}</el-button>
            </div>
          </div>
        </div>

        <WfTable v-loading="loading" :data="tableData" class="wf-list-panel__table" :row-key="rowKey">
          <el-table-column prop="userName" :label="t('system.session.colUser')" min-width="120" show-overflow-tooltip />
          <el-table-column prop="nickName" :label="t('system.session.colNick')" min-width="120" show-overflow-tooltip>
            <template #default="{ row }">{{ row.nickName || t('common.none') }}</template>
          </el-table-column>
          <el-table-column prop="clientType" :label="t('system.session.colClient')" width="108" align="center">
            <template #default="{ row }">
              <el-tag :type="clientTypeTag(row.clientType)" size="small" disable-transitions>
                {{ clientTypeLabel(row.clientType) }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="loginTime" :label="t('system.session.colLoginTime')" width="148" show-overflow-tooltip>
            <template #default="{ row }">
              <span class="wf-session-page__time">{{ formatDateShort(row.loginTime) }}</span>
            </template>
          </el-table-column>
          <el-table-column prop="expireTime" :label="t('system.session.colExpireTime')" width="148" show-overflow-tooltip>
            <template #default="{ row }">
              <span class="wf-session-page__time">{{ formatDateShort(row.expireTime) }}</span>
            </template>
          </el-table-column>
          <el-table-column prop="isActive" :label="t('system.session.colStatus')" width="88" align="center">
            <template #default="{ row }">
              <el-tag :type="row.isActive ? 'success' : 'info'" size="small" disable-transitions>
                {{ row.isActive ? t('system.session.statusActive') : t('system.session.statusInactive') }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column
            v-if="canKick"
            :label="t('common.actions')"
            width="120"
            fixed="right"
            align="center"
            class-name="wf-session-page__actions-cell"
          >
            <template #default="{ row }">
              <div class="wf-session-page__actions">
                <el-button
                  link
                  type="danger"
                  :disabled="!row.isActive"
                  :loading="kickingKey === rowKey(row)"
                  @click="handleKick(row)"
                >
                  <el-icon><SwitchButton /></el-icon>
                  {{ t('system.session.kick') }}
                </el-button>
              </div>
            </template>
          </el-table-column>
          <template #empty>
            <el-empty :description="t('system.session.emptyData')" :image-size="96" />
          </template>
        </WfTable>
      </div>

      <template #pager>
        <WfPagePager
          v-model:current-page="query.pageIndex"
          v-model:page-size="query.pageSize"
          :total="total"
          @change="loadData"
          @size-change="handleSearch"
        />
      </template>
    </WfPageBody>
  </WfPage>
</template>

<style scoped lang="scss">
.wf-session-page {
  &__body {
    :deep(.el-card__body) {
      padding: 0;
      display: flex;
      flex-direction: column;
    }
  }

  &__only-active {
    height: 32px;
    margin-right: 0;
    white-space: nowrap;
  }

  &__time {
    font-size: 12px;
    color: var(--wf-text-secondary);
    font-variant-numeric: tabular-nums;
  }

  &__actions {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    flex-wrap: wrap;
    gap: 2px 4px;
  }
}
</style>
