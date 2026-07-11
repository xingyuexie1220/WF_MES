<script setup lang="ts">
defineOptions({ name: 'SystemDict' })
import { computed, nextTick, onMounted, reactive, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { ElMessage, ElMessageBox } from 'element-plus'
import { CirclePlusFilled, Collection, Search } from '@element-plus/icons-vue'
import { invalidateDictCache } from '@/composables/useDict'
import {
  createDictDataApi,
  createDictTypeApi,
  deleteDictDataApi,
  deleteDictTypeApi,
  getDictDataPageApi,
  getDictTypePageApi,
  updateDictDataApi,
  updateDictTypeApi
} from '@/api/system/dict'
import { WfDialogFooter, WfPage, WfPageBody, WfPagePager, WfStatusTag, WfTable } from '@/components/page'
import type { DictDataItem, DictTypeItem } from '@/types/system/dict'

const { t } = useI18n()
const typeTableRef = ref<InstanceType<typeof WfTable>>()
const typeLoading = ref(false)
const dataLoading = ref(false)
const typeSubmitting = ref(false)
const dataSubmitting = ref(false)
const typeData = ref<DictTypeItem[]>([])
const dictData = ref<DictDataItem[]>([])
const typeTotal = ref(0)
const dataTotal = ref(0)
const selectedType = ref<DictTypeItem | null>(null)
const typeDialogVisible = ref(false)
const dataDialogVisible = ref(false)
const editingTypeId = ref<number | null>(null)
const editingDataId = ref<number | null>(null)

const typeQuery = reactive({
  pageIndex: 1,
  pageSize: 10,
  dictName: '',
  dictType: ''
})

const dataQuery = reactive({
  pageIndex: 1,
  pageSize: 20,
  dictLabel: ''
})

const typeForm = reactive({
  dictName: '',
  dictType: '',
  status: 1,
  remark: ''
})

const dataForm = reactive({
  dictLabel: '',
  dictValue: '',
  sort: 1,
  status: 1,
  remark: ''
})

const typeDialogTitle = computed(() =>
  editingTypeId.value ? t('system.dict.editType') : t('system.dict.addType')
)

const dataDialogTitle = computed(() =>
  editingDataId.value ? t('system.dict.editData') : t('system.dict.addData')
)

const dataPanelTitle = computed(() =>
  selectedType.value
    ? t('system.dict.dataPanel', { name: selectedType.value.dictName })
    : t('system.dict.dataPanelEmpty')
)

async function syncTypeTableCurrentRow(row: DictTypeItem | null) {
  await nextTick()
  typeTableRef.value?.tableRef?.setCurrentRow(row ?? undefined)
}

async function loadTypes() {
  typeLoading.value = true
  try {
    const result = await getDictTypePageApi({ ...typeQuery })
    typeData.value = result.items
    typeTotal.value = result.total

    let target = selectedType.value
      ? result.items.find((item) => item.id === selectedType.value!.id) ?? null
      : null

    if (!target && result.items.length > 0) {
      target = result.items[0]
    }

    selectedType.value = target
    await syncTypeTableCurrentRow(target)

    if (target) {
      await loadDictData()
    } else {
      dictData.value = []
      dataTotal.value = 0
    }
  } finally {
    typeLoading.value = false
  }
}

async function loadDictData() {
  if (!selectedType.value) {
    dictData.value = []
    dataTotal.value = 0
    return
  }
  dataLoading.value = true
  try {
    const result = await getDictDataPageApi({
      ...dataQuery,
      dictTypeId: selectedType.value.id,
      dictType: selectedType.value.dictType
    })
    dictData.value = result.items
    dataTotal.value = result.total
  } finally {
    dataLoading.value = false
  }
}

function selectType(row: DictTypeItem) {
  selectedType.value = row
  dataQuery.pageIndex = 1
  void syncTypeTableCurrentRow(row)
  void loadDictData()
}

function handleTypeSearch() {
  typeQuery.pageIndex = 1
  void loadTypes()
}

function resetTypeSearch() {
  typeQuery.dictName = ''
  typeQuery.dictType = ''
  typeQuery.pageIndex = 1
  void loadTypes()
}

function handleDataSearch() {
  if (!selectedType.value) {
    ElMessage.warning(t('system.dict.selectTypeFirst'))
    return
  }
  dataQuery.pageIndex = 1
  void loadDictData()
}

function resetDataSearch() {
  dataQuery.dictLabel = ''
  dataQuery.pageIndex = 1
  void loadDictData()
}

function openCreateType() {
  editingTypeId.value = null
  Object.assign(typeForm, { dictName: '', dictType: '', status: 1, remark: '' })
  typeDialogVisible.value = true
}

function openEditType(row: DictTypeItem) {
  editingTypeId.value = row.id
  Object.assign(typeForm, {
    dictName: row.dictName,
    dictType: row.dictType,
    status: row.status,
    remark: row.remark ?? ''
  })
  typeDialogVisible.value = true
}

function openCreateData() {
  if (!selectedType.value) {
    ElMessage.warning(t('system.dict.selectTypeFirst'))
    return
  }
  editingDataId.value = null
  Object.assign(dataForm, { dictLabel: '', dictValue: '', sort: 1, status: 1, remark: '' })
  dataDialogVisible.value = true
}

function openEditData(row: DictDataItem) {
  editingDataId.value = row.id
  Object.assign(dataForm, {
    dictLabel: row.dictLabel,
    dictValue: row.dictValue,
    sort: row.sort,
    status: row.status,
    remark: row.remark ?? ''
  })
  dataDialogVisible.value = true
}

async function submitType() {
  if (!typeForm.dictName.trim() || !typeForm.dictType.trim()) {
    ElMessage.warning(t('system.dict.validateType'))
    return
  }
  typeSubmitting.value = true
  try {
    if (editingTypeId.value) {
      await updateDictTypeApi(editingTypeId.value, {
        dictName: typeForm.dictName,
        status: typeForm.status,
        remark: typeForm.remark
      })
    } else {
      await createDictTypeApi({ ...typeForm })
    }
    ElMessage.success(t('common.saveSuccess'))
    typeDialogVisible.value = false
    await loadTypes()
  } finally {
    typeSubmitting.value = false
  }
}

async function submitData() {
  if (!selectedType.value || !dataForm.dictLabel.trim() || !dataForm.dictValue.trim()) {
    ElMessage.warning(t('system.dict.validateData'))
    return
  }
  dataSubmitting.value = true
  try {
    if (editingDataId.value) {
      await updateDictDataApi(editingDataId.value, { ...dataForm })
    } else {
      await createDictDataApi({ ...dataForm, dictTypeId: selectedType.value.id })
    }
    ElMessage.success(t('common.saveSuccess'))
    dataDialogVisible.value = false
    invalidateDictCache(selectedType.value?.dictType)
    await loadDictData()
  } finally {
    dataSubmitting.value = false
  }
}

async function handleDeleteType(row: DictTypeItem) {
  await ElMessageBox.confirm(t('system.dict.confirmDeleteType', { name: row.dictName }), t('common.confirm'))
  await deleteDictTypeApi(row.id)
  if (selectedType.value?.id === row.id) {
    selectedType.value = null
  }
  ElMessage.success(t('common.deleteSuccess'))
  await loadTypes()
}

async function handleDeleteData(row: DictDataItem) {
  await ElMessageBox.confirm(t('system.dict.confirmDeleteData', { name: row.dictLabel }), t('common.confirm'))
  await deleteDictDataApi(row.id)
  invalidateDictCache(row.dictType)
  ElMessage.success(t('common.deleteSuccess'))
  await loadDictData()
}

onMounted(loadTypes)
</script>

<template>
  <WfPage>
    <WfPageBody class="wf-dict-page__body-wrap">
      <div class="wf-dict-page">
        <header class="wf-dict-page__header">
          <div class="wf-dict-page__header-main">
            <span class="wf-dict-page__header-icon">
              <el-icon><Collection /></el-icon>
            </span>
            <div class="wf-dict-page__header-text">
              <h2 class="wf-dict-page__header-title">{{ t('system.dict.title') }}</h2>
              <p class="wf-dict-page__header-desc">{{ t('system.dict.pageDesc') }}</p>
            </div>
          </div>
          <span v-if="typeTotal > 0" class="wf-dict-page__header-badge">
            {{ t('system.dict.typeTotal', { count: typeTotal }) }}
          </span>
        </header>

        <div class="wf-dict-page__body">
          <section class="wf-dict-page__panel">
            <div class="wf-dict-page__panel-head">
              <span class="wf-dict-page__panel-title">{{ t('system.dict.typePanel') }}</span>
              <span class="wf-dict-page__panel-meta">{{ t('system.dict.typeTotal', { count: typeTotal }) }}</span>
            </div>

            <div class="wf-dict-page__panel-toolbar">
              <div class="wf-dict-page__panel-filters">
                <el-input
                  v-model="typeQuery.dictName"
                  clearable
                  :placeholder="t('system.dict.dictName')"
                  class="wf-dict-page__filter-input"
                  @keyup.enter="handleTypeSearch"
                />
                <el-input
                  v-model="typeQuery.dictType"
                  clearable
                  :placeholder="t('system.dict.dictType')"
                  class="wf-dict-page__filter-input"
                  @keyup.enter="handleTypeSearch"
                />
              </div>
              <div class="wf-dict-page__panel-actions">
                <el-button type="primary" :icon="Search" @click="handleTypeSearch">{{ t('common.query') }}</el-button>
                <el-button @click="resetTypeSearch">{{ t('common.reset') }}</el-button>
                <el-button type="success" :icon="CirclePlusFilled" @click="openCreateType">{{ t('system.dict.addType') }}</el-button>
              </div>
            </div>

            <WfTable
              ref="typeTableRef"
              v-loading="typeLoading"
              :data="typeData"
              class="wf-dict-page__table wf-dict-page__table--types"
              highlight-current-row
              row-key="id"
              @row-click="selectType"
            >
              <el-table-column prop="dictName" :label="t('system.dict.dictName')" min-width="110" show-overflow-tooltip />
              <el-table-column prop="dictType" :label="t('system.dict.dictType')" min-width="140" show-overflow-tooltip>
                <template #default="{ row }">
                  <code class="wf-dict-page__code">{{ row.dictType }}</code>
                </template>
              </el-table-column>
              <el-table-column prop="status" :label="t('common.status')" width="80" align="center">
                <template #default="{ row }">
                  <WfStatusTag :status="row.status" />
                </template>
              </el-table-column>
              <el-table-column :label="t('common.actions')" width="110" fixed="right" align="center">
                <template #default="{ row }">
                  <el-button link type="primary" @click.stop="openEditType(row)">{{ t('common.edit') }}</el-button>
                  <el-button link type="danger" @click.stop="handleDeleteType(row)">{{ t('common.delete') }}</el-button>
                </template>
              </el-table-column>
            </WfTable>

            <WfPagePager
              v-model:current-page="typeQuery.pageIndex"
              v-model:page-size="typeQuery.pageSize"
              :total="typeTotal"
              :page-sizes="[10, 20, 50]"
              class="wf-dict-page__pager"
              @change="loadTypes"
            />
          </section>

          <section class="wf-dict-page__panel" :class="{ 'is-dimmed': !selectedType }">
            <div class="wf-dict-page__panel-head">
              <div class="wf-dict-page__panel-title-wrap">
                <span class="wf-dict-page__panel-title">{{ dataPanelTitle }}</span>
                <code v-if="selectedType" class="wf-dict-page__code wf-dict-page__code--accent">
                  {{ t('system.dict.dataPanelCode', { code: selectedType.dictType }) }}
                </code>
              </div>
              <span v-if="selectedType" class="wf-dict-page__panel-meta">
                {{ t('system.dict.dataTotal', { count: dataTotal }) }}
              </span>
            </div>

            <template v-if="selectedType">
              <div class="wf-dict-page__panel-toolbar">
                <div class="wf-dict-page__panel-filters">
                  <el-input
                    v-model="dataQuery.dictLabel"
                    clearable
                    :placeholder="t('system.dict.dictLabel')"
                    class="wf-dict-page__filter-input"
                    @keyup.enter="handleDataSearch"
                  />
                  <span class="wf-dict-page__panel-hint">{{ t('system.dict.dataPanelHint') }}</span>
                </div>
                <div class="wf-dict-page__panel-actions">
                  <el-button type="primary" :icon="Search" @click="handleDataSearch">{{ t('common.query') }}</el-button>
                  <el-button @click="resetDataSearch">{{ t('common.reset') }}</el-button>
                  <el-button type="success" :icon="CirclePlusFilled" @click="openCreateData">{{ t('system.dict.addData') }}</el-button>
                </div>
              </div>

              <WfTable v-loading="dataLoading" :data="dictData" class="wf-dict-page__table">
                <el-table-column prop="dictLabel" :label="t('system.dict.dictLabel')" min-width="120" />
                <el-table-column prop="dictValue" :label="t('system.dict.dictValue')" min-width="100">
                  <template #default="{ row }">
                    <code class="wf-dict-page__code">{{ row.dictValue }}</code>
                  </template>
                </el-table-column>
                <el-table-column prop="sort" :label="t('common.sort')" width="70" align="center" />
                <el-table-column prop="status" :label="t('common.status')" width="80" align="center">
                  <template #default="{ row }">
                    <WfStatusTag :status="row.status" />
                  </template>
                </el-table-column>
                <el-table-column prop="remark" :label="t('common.remark')" min-width="120" show-overflow-tooltip />
                <el-table-column :label="t('common.actions')" width="110" fixed="right" align="center">
                  <template #default="{ row }">
                    <el-button link type="primary" @click="openEditData(row)">{{ t('common.edit') }}</el-button>
                    <el-button link type="danger" @click="handleDeleteData(row)">{{ t('common.delete') }}</el-button>
                  </template>
                </el-table-column>
                <template #empty>
                  <el-empty :description="t('system.dict.emptyData')" :image-size="72" />
                </template>
              </WfTable>

              <WfPagePager
                v-model:current-page="dataQuery.pageIndex"
                v-model:page-size="dataQuery.pageSize"
                :total="dataTotal"
                class="wf-dict-page__pager"
                @change="loadDictData"
              />
            </template>

            <div v-else class="wf-dict-page__placeholder">
              <el-empty :description="t('system.dict.selectHint')" :image-size="88">
                <p class="wf-dict-page__placeholder-tip">{{ t('system.dict.dataPanelHint') }}</p>
              </el-empty>
            </div>
          </section>
        </div>
      </div>
    </WfPageBody>

    <el-dialog v-model="typeDialogVisible" :title="typeDialogTitle" width="520px" destroy-on-close align-center>
      <el-form label-width="96px">
        <el-form-item :label="t('system.dict.dictName')" required>
          <el-input v-model="typeForm.dictName" />
        </el-form-item>
        <el-form-item :label="t('system.dict.dictType')" required>
          <el-input v-model="typeForm.dictType" :disabled="Boolean(editingTypeId)" placeholder="sys_xxx_yyy" />
        </el-form-item>
        <el-form-item :label="t('common.status')">
          <el-radio-group v-model="typeForm.status">
            <el-radio :value="1">{{ t('common.enabled') }}</el-radio>
            <el-radio :value="0">{{ t('common.disabled') }}</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item :label="t('common.remark')">
          <el-input v-model="typeForm.remark" type="textarea" :rows="3" />
        </el-form-item>
      </el-form>
      <template #footer>
        <WfDialogFooter :loading="typeSubmitting" @cancel="typeDialogVisible = false" @confirm="submitType" />
      </template>
    </el-dialog>

    <el-dialog v-model="dataDialogVisible" :title="dataDialogTitle" width="520px" destroy-on-close align-center>
      <el-form label-width="96px">
        <el-form-item v-if="selectedType" :label="t('system.dict.dictType')">
          <el-input :model-value="selectedType.dictType" disabled />
        </el-form-item>
        <el-form-item :label="t('system.dict.dictLabel')" required>
          <el-input v-model="dataForm.dictLabel" />
        </el-form-item>
        <el-form-item :label="t('system.dict.dictValue')" required>
          <el-input v-model="dataForm.dictValue" />
        </el-form-item>
        <el-form-item :label="t('common.sort')">
          <el-input-number v-model="dataForm.sort" :min="0" />
        </el-form-item>
        <el-form-item :label="t('common.status')">
          <el-radio-group v-model="dataForm.status">
            <el-radio :value="1">{{ t('common.enabled') }}</el-radio>
            <el-radio :value="0">{{ t('common.disabled') }}</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item :label="t('common.remark')">
          <el-input v-model="dataForm.remark" type="textarea" :rows="3" />
        </el-form-item>
      </el-form>
      <template #footer>
        <WfDialogFooter :loading="dataSubmitting" @cancel="dataDialogVisible = false" @confirm="submitData" />
      </template>
    </el-dialog>
  </WfPage>
</template>

<style scoped lang="scss">
.wf-dict-page {
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

  &__header-badge {
    flex-shrink: 0;
    padding: 4px 12px;
    font-size: 12px;
    font-weight: 600;
    color: var(--wf-tab-primary);
    background: var(--wf-tab-active-bg);
    border-radius: 999px;
    white-space: nowrap;
  }

  &__body {
    flex: 1;
    min-height: 0;
    display: grid;
    grid-template-columns: minmax(360px, 42%) 1fr;
    gap: 12px;
  }

  &__panel {
    min-height: 0;
    display: flex;
    flex-direction: column;
    padding: 12px 14px;
    background: #fff;
    border: 1px solid #e8eaed;
    border-radius: 8px;
    box-shadow: var(--wf-card-shadow);

    &.is-dimmed {
      background: #fafbfc;
    }
  }

  &__panel-head {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    padding-bottom: 10px;
    margin-bottom: 10px;
    border-bottom: 1px solid #eef0f3;
    flex-shrink: 0;
  }

  &__panel-title-wrap {
    display: flex;
    align-items: center;
    gap: 8px;
    min-width: 0;
    flex-wrap: wrap;
  }

  &__panel-title {
    display: inline-flex;
    align-items: center;
    gap: 8px;
    font-size: 14px;
    font-weight: 600;
    color: var(--wf-text);

    &::before {
      content: '';
      width: 3px;
      height: 14px;
      border-radius: 2px;
      background: var(--wf-tab-primary);
      flex-shrink: 0;
    }
  }

  &__panel-meta {
    font-size: 12px;
    color: var(--wf-text-muted);
    white-space: nowrap;
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

  &__panel-hint {
    font-size: 12px;
    color: var(--wf-text-muted);
    white-space: nowrap;
  }

  &__filter-input {
    width: 132px;
  }

  &__table {
    flex: 1;
    min-height: 0;

    &--types {
      :deep(.el-table__body tr) {
        cursor: pointer;
      }

      :deep(.el-table__body tr.current-row > td) {
        background: var(--wf-tab-active-bg) !important;
      }
    }
  }

  &__pager {
    flex-shrink: 0;
    display: flex;
    justify-content: flex-end;
    margin-top: 10px;
    padding-top: 8px;
    border-top: 1px solid #f0f2f5;
  }

  &__placeholder {
    flex: 1;
    min-height: 200px;
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 24px 12px;
  }

  &__placeholder-tip {
    margin: 8px 0 0;
    font-size: 12px;
    color: var(--wf-text-muted);
    text-align: center;
  }

  &__code {
    display: inline-block;
    max-width: 100%;
    padding: 2px 8px;
    font-family: ui-monospace, 'Cascadia Code', Consolas, monospace;
    font-size: 12px;
    color: #4e5969;
    background: #f2f3f5;
    border: 1px solid #e8eaed;
    border-radius: 4px;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    vertical-align: middle;

    &--accent {
      color: var(--wf-tab-primary);
      background: var(--wf-tab-active-bg);
      border-color: #d9e1ff;
    }
  }
}

@media (max-width: 1100px) {
  .wf-dict-page {
    &__body {
      grid-template-columns: 1fr;
    }

    &__panel-toolbar {
      grid-template-columns: 1fr;
    }

    &__panel-actions {
      justify-content: flex-start;
    }
  }
}
</style>
