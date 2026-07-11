<script setup lang="ts">
defineOptions({ name: 'SystemNotice' })
import { computed, onMounted, reactive, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Bell, CirclePlusFilled, Search } from '@element-plus/icons-vue'
import {
  createNoticeApi,
  deleteNoticeApi,
  getNoticePageApi,
  publishNoticeApi,
  updateNoticeApi
} from '@/api/system/notice'
import { useDict } from '@/composables/useDict'
import { WfDialogFooter, WfPage, WfPageBody, WfPagePager, WfTable } from '@/components/page'
import type { NoticeItem } from '@/types/system/notice'

const { t } = useI18n()
const {
  options: noticeTypeOptions,
  loading: noticeTypeLoading,
  load: loadNoticeTypes,
  labelOf: noticeTypeLabel,
  defaultValue: defaultNoticeType
} = useDict('sys_notice_type')
const {
  options: noticeStatusOptions,
  load: loadNoticeStatuses,
  labelOf: noticeStatusLabel,
  defaultValue: defaultNoticeStatus
} = useDict('sys_notice_status')

const loading = ref(false)
const submitting = ref(false)
const tableData = ref<NoticeItem[]>([])
const total = ref(0)
const dialogVisible = ref(false)
const editingId = ref<number | null>(null)

const query = reactive({
  pageIndex: 1,
  pageSize: 20,
  title: '',
  noticeType: undefined as number | undefined,
  status: undefined as number | undefined
})

const form = reactive({
  title: '',
  content: '',
  noticeType: 1,
  status: 0
})

const dialogTitle = computed(() =>
  editingId.value ? t('system.notice.edit') : t('system.notice.add')
)

async function refreshDictOptions() {
  await Promise.all([loadNoticeTypes(true), loadNoticeStatuses(true)])
}

function formatDate(value?: string) {
  if (!value) return t('common.none')
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return value
  return date.toLocaleString()
}

async function loadData() {
  loading.value = true
  try {
    const result = await getNoticePageApi({ ...query })
    tableData.value = result.items
    total.value = result.total
  } finally {
    loading.value = false
  }
}

function handleSearch() {
  query.pageIndex = 1
  loadData()
}

function handleReset() {
  query.title = ''
  query.noticeType = undefined
  query.status = undefined
  query.pageIndex = 1
  loadData()
}

async function openCreate() {
  await refreshDictOptions()
  editingId.value = null
  Object.assign(form, {
    title: '',
    content: '',
    noticeType: defaultNoticeType(1),
    status: defaultNoticeStatus(0)
  })
  dialogVisible.value = true
}

async function openEdit(row: NoticeItem) {
  await refreshDictOptions()
  editingId.value = row.id
  Object.assign(form, {
    title: row.title,
    content: row.content,
    noticeType: row.noticeType,
    status: row.status
  })
  dialogVisible.value = true
}

async function submitForm() {
  if (!form.title.trim() || !form.content.trim()) {
    ElMessage.warning(t('system.notice.validateRequired'))
    return
  }
  submitting.value = true
  try {
    if (editingId.value) {
      await updateNoticeApi(editingId.value, { ...form })
    } else {
      await createNoticeApi({ ...form })
    }
    ElMessage.success(t('common.saveSuccess'))
    dialogVisible.value = false
    await loadData()
  } finally {
    submitting.value = false
  }
}

async function handlePublish(row: NoticeItem) {
  await publishNoticeApi(row.id)
  ElMessage.success(t('system.notice.publishSuccess'))
  await loadData()
}

async function handleDelete(row: NoticeItem) {
  await ElMessageBox.confirm(t('system.notice.confirmDelete', { name: row.title }), t('common.confirm'))
  await deleteNoticeApi(row.id)
  ElMessage.success(t('common.deleteSuccess'))
  await loadData()
}

onMounted(async () => {
  await Promise.all([loadNoticeTypes(), loadNoticeStatuses()])
  await loadData()
})
</script>

<template>
  <WfPage class="wf-notice-page">
    <WfPageBody class="wf-notice-page__body">
      <div class="wf-list-panel">
        <div class="wf-list-panel__head">
          <div class="wf-list-panel__title-block">
            <div class="wf-list-panel__title">
              <el-icon class="wf-list-panel__title-icon"><Bell /></el-icon>
              {{ t('system.notice.title') }}
              <span class="wf-list-panel__count">{{ t('system.notice.noticeTotal', { count: total }) }}</span>
            </div>
            <p class="wf-list-panel__desc">{{ t('system.notice.pageDesc') }}</p>
          </div>
          <div class="wf-list-panel__toolbar">
            <div class="wf-list-panel__group wf-list-panel__group--filter">
              <el-input
                v-model="query.title"
                clearable
                :placeholder="t('system.notice.filterTitle')"
                class="wf-list-panel__input wf-list-panel__input--wide"
                @keyup.enter="handleSearch"
              />
              <el-select v-model="query.noticeType" clearable :placeholder="t('system.notice.colType')" class="wf-list-panel__select">
                <el-option v-for="item in noticeTypeOptions" :key="item.dictValue" :label="item.dictLabel" :value="Number(item.dictValue)" />
              </el-select>
              <el-select v-model="query.status" clearable :placeholder="t('common.status')" class="wf-list-panel__select">
                <el-option v-for="item in noticeStatusOptions" :key="item.dictValue" :label="item.dictLabel" :value="Number(item.dictValue)" />
              </el-select>
            </div>
            <span class="wf-list-panel__divider" aria-hidden="true" />
            <div class="wf-list-panel__group wf-list-panel__group--query">
              <el-button type="primary" :icon="Search" @click="handleSearch">{{ t('common.query') }}</el-button>
            </div>
            <span class="wf-list-panel__divider" aria-hidden="true" />
            <div class="wf-list-panel__group wf-list-panel__group--action">
              <el-button type="success" :icon="CirclePlusFilled" @click="openCreate">{{ t('system.notice.add') }}</el-button>
              <el-button @click="handleReset">{{ t('common.reset') }}</el-button>
            </div>
          </div>
        </div>

        <WfTable v-loading="loading" :data="tableData" class="wf-list-panel__table">
          <el-table-column prop="title" :label="t('system.notice.colTitle')" min-width="180" show-overflow-tooltip />
          <el-table-column prop="noticeType" :label="t('system.notice.colType')" width="100">
            <template #default="{ row }">{{ noticeTypeLabel(row.noticeType) }}</template>
          </el-table-column>
          <el-table-column prop="status" :label="t('common.status')" width="100">
            <template #default="{ row }">
              <el-tag :type="row.status === 1 ? 'success' : 'info'" size="small">
                {{ noticeStatusLabel(row.status) }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="publishTime" :label="t('system.notice.colPublishTime')" width="170">
            <template #default="{ row }">{{ formatDate(row.publishTime) }}</template>
          </el-table-column>
          <el-table-column prop="createByName" :label="t('common.createBy')" width="100" />
          <el-table-column :label="t('common.actions')" width="220" fixed="right">
            <template #default="{ row }">
              <el-button link type="primary" @click="openEdit(row)">{{ t('common.edit') }}</el-button>
              <el-button v-if="row.status !== 1" link type="primary" @click="handlePublish(row)">{{ t('system.notice.publish') }}</el-button>
              <el-button link type="danger" @click="handleDelete(row)">{{ t('common.delete') }}</el-button>
            </template>
          </el-table-column>
        </WfTable>

        <WfPagePager v-model:current-page="query.pageIndex" v-model:page-size="query.pageSize" :total="total" @change="loadData" />
      </div>
    </WfPageBody>

    <el-dialog v-model="dialogVisible" :title="dialogTitle" width="640px" destroy-on-close align-center>
      <el-form label-width="88px">
        <el-form-item :label="t('system.notice.colTitle')" required>
          <el-input v-model="form.title" maxlength="200" show-word-limit />
        </el-form-item>
        <el-form-item :label="t('system.notice.colType')">
          <el-select v-model="form.noticeType" style="width: 100%" :loading="noticeTypeLoading">
            <el-option v-for="item in noticeTypeOptions" :key="item.dictValue" :label="item.dictLabel" :value="Number(item.dictValue)" />
          </el-select>
        </el-form-item>
        <el-form-item :label="t('common.status')">
          <el-radio-group v-model="form.status">
            <el-radio v-for="item in noticeStatusOptions" :key="item.dictValue" :value="Number(item.dictValue)">
              {{ item.dictLabel }}
            </el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item :label="t('system.notice.colContent')" required>
          <el-input v-model="form.content" type="textarea" :rows="8" maxlength="5000" show-word-limit />
        </el-form-item>
      </el-form>
      <template #footer>
        <WfDialogFooter :loading="submitting" @cancel="dialogVisible = false" @confirm="submitForm" />
      </template>
    </el-dialog>
  </WfPage>
</template>

<style scoped lang="scss">
.wf-notice-page {
  &__body {
    :deep(.el-card__body) {
      padding: 0;
      display: flex;
      flex-direction: column;
    }
  }
}
</style>
