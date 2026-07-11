<script setup lang="ts">
defineOptions({ name: 'SystemPosition' })
import { onMounted, reactive, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Briefcase, CirclePlusFilled, Search } from '@element-plus/icons-vue'
import { getDeptTreeApi } from '@/api/system/dept'
import { createPositionApi, deletePositionApi, getPositionPageApi, updatePositionApi } from '@/api/system/position'
import { WfDialogFooter, WfPage, WfPageBody, WfPagePager, WfStatusTag, WfTable } from '@/components/page'
import type { DeptItem } from '@/types/system/dept'
import type { PositionItem } from '@/types/system/position'

const { t } = useI18n()
const loading = ref(false)
const submitting = ref(false)
const tableData = ref<PositionItem[]>([])
const total = ref(0)
const deptTree = ref<DeptItem[]>([])
const dialogVisible = ref(false)
const editingId = ref<number | null>(null)

const query = reactive({ pageIndex: 1, pageSize: 20, positionName: '' })
const form = reactive({
  positionCode: '',
  positionName: '',
  processCode: '',
  deptId: undefined as number | undefined,
  sort: 1,
  status: 1,
  remark: ''
})

async function loadData() {
  loading.value = true
  try {
    const data = await getPositionPageApi({ ...query })
    tableData.value = data.items
    total.value = data.total
  } finally {
    loading.value = false
  }
}

async function loadOptions() {
  deptTree.value = await getDeptTreeApi()
}

function handleSearch() {
  query.pageIndex = 1
  loadData()
}

function handleReset() {
  query.positionName = ''
  query.pageIndex = 1
  loadData()
}

function openCreate() {
  editingId.value = null
  Object.assign(form, {
    positionCode: '',
    positionName: '',
    processCode: '',
    deptId: undefined,
    sort: 1,
    status: 1,
    remark: ''
  })
  dialogVisible.value = true
}

function openEdit(row: PositionItem) {
  editingId.value = row.id
  Object.assign(form, row)
  dialogVisible.value = true
}

async function submitForm() {
  if (!form.positionCode.trim() || !form.positionName.trim()) {
    ElMessage.warning(t('system.position.validateRequired'))
    return
  }
  submitting.value = true
  try {
    if (editingId.value) {
      await updatePositionApi(editingId.value, { ...form })
      ElMessage.success(t('common.updateSuccess'))
    } else {
      await createPositionApi({ ...form })
      ElMessage.success(t('common.createSuccess'))
    }
    dialogVisible.value = false
    await loadData()
  } finally {
    submitting.value = false
  }
}

async function handleDelete(row: PositionItem) {
  await ElMessageBox.confirm(t('system.position.confirmDelete', { name: row.positionName }), t('common.tip'), {
    type: 'warning'
  })
  await deletePositionApi(row.id)
  ElMessage.success(t('common.deleteSuccess'))
  await loadData()
}

onMounted(async () => {
  await loadOptions()
  await loadData()
})
</script>

<template>
  <WfPage class="wf-position-page">
    <WfPageBody class="wf-position-page__body">
      <div class="wf-list-panel">
        <div class="wf-list-panel__head">
          <div class="wf-list-panel__title-block">
            <div class="wf-list-panel__title">
              <el-icon class="wf-list-panel__title-icon"><Briefcase /></el-icon>
              {{ t('system.position.title') }}
              <span class="wf-list-panel__count">{{ t('system.position.positionTotal', { count: total }) }}</span>
            </div>
            <p class="wf-list-panel__desc">{{ t('system.position.pageDesc') }}</p>
          </div>
          <div class="wf-list-panel__toolbar">
            <div class="wf-list-panel__group wf-list-panel__group--filter">
              <el-input
                v-model="query.positionName"
                clearable
                :placeholder="t('system.position.positionName')"
                class="wf-list-panel__input wf-list-panel__input--wide"
                @keyup.enter="handleSearch"
              />
            </div>
            <span class="wf-list-panel__divider" aria-hidden="true" />
            <div class="wf-list-panel__group wf-list-panel__group--query">
              <el-button type="primary" :icon="Search" @click="handleSearch">{{ t('common.query') }}</el-button>
            </div>
            <span class="wf-list-panel__divider" aria-hidden="true" />
            <div class="wf-list-panel__group wf-list-panel__group--action">
              <el-button type="success" :icon="CirclePlusFilled" @click="openCreate">
                {{ t('system.position.add') }}
              </el-button>
              <el-button @click="handleReset">{{ t('common.reset') }}</el-button>
            </div>
          </div>
        </div>

        <WfTable v-loading="loading" :data="tableData" class="wf-position-table">
          <el-table-column prop="positionCode" :label="t('system.position.positionCode')" min-width="120" show-overflow-tooltip />
          <el-table-column prop="positionName" :label="t('system.position.positionName')" min-width="120" show-overflow-tooltip />
          <el-table-column prop="processCode" :label="t('system.position.processCode')" min-width="120" show-overflow-tooltip />
          <el-table-column prop="deptName" :label="t('system.position.belongDept')" min-width="140" show-overflow-tooltip />
          <el-table-column prop="sort" :label="t('common.sort')" width="72" align="center" />
          <el-table-column prop="status" :label="t('common.status')" width="88" align="center">
            <template #default="{ row }">
              <WfStatusTag :status="row.status" />
            </template>
          </el-table-column>
          <el-table-column :label="t('common.actions')" width="160" fixed="right" align="center">
            <template #default="{ row }">
              <el-button link type="primary" @click="openEdit(row)">{{ t('common.edit') }}</el-button>
              <el-button link type="danger" @click="handleDelete(row)">{{ t('common.delete') }}</el-button>
            </template>
          </el-table-column>
        </WfTable>

        <WfPagePager
          v-model:current-page="query.pageIndex"
          v-model:page-size="query.pageSize"
          :total="total"
          @change="loadData"
        />
      </div>
    </WfPageBody>

    <el-dialog
      v-model="dialogVisible"
      :title="editingId ? t('system.position.edit') : t('system.position.add')"
      width="560px"
      destroy-on-close
      align-center
    >
      <el-form :model="form" label-width="90px">
        <el-form-item :label="t('system.position.positionCode')" required>
          <el-input v-model="form.positionCode" :disabled="!!editingId" />
        </el-form-item>
        <el-form-item :label="t('system.position.positionName')" required>
          <el-input v-model="form.positionName" />
        </el-form-item>
        <el-form-item :label="t('system.position.processCode')">
          <el-input v-model="form.processCode" :placeholder="t('system.position.processCodePlaceholder')" />
        </el-form-item>
        <el-form-item :label="t('system.position.belongDept')">
          <el-tree-select
            v-model="form.deptId"
            :data="deptTree"
            node-key="id"
            :props="{ label: 'deptName', value: 'id', children: 'children' }"
            check-strictly
            clearable
            style="width: 100%"
          />
        </el-form-item>
        <el-form-item :label="t('common.sort')">
          <el-input-number v-model="form.sort" :min="0" controls-position="right" style="width: 100%" />
        </el-form-item>
        <el-form-item :label="t('common.status')">
          <el-radio-group v-model="form.status">
            <el-radio :value="1">{{ t('common.enabled') }}</el-radio>
            <el-radio :value="0">{{ t('common.disabled') }}</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item :label="t('common.remark')">
          <el-input v-model="form.remark" type="textarea" :rows="3" />
        </el-form-item>
      </el-form>
      <template #footer>
        <WfDialogFooter :loading="submitting" @cancel="dialogVisible = false" @confirm="submitForm" />
      </template>
    </el-dialog>
  </WfPage>
</template>

<style scoped lang="scss">
.wf-position-page {
  &__body {
    :deep(.el-card__body) {
      padding: 0;
      display: flex;
      flex-direction: column;
    }
  }
}

.wf-list-panel {
  flex: 1;
  min-height: 0;
  display: flex;
  flex-direction: column;
  padding: 12px;

  &__head {
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: 16px;
    margin-bottom: 10px;
    flex-wrap: wrap;
  }

  &__title-block {
    flex: 1;
    min-width: 240px;
  }

  &__title {
    display: inline-flex;
    align-items: center;
    gap: 8px;
    font-size: 15px;
    font-weight: 600;
    color: var(--wf-text);
  }

  &__title-icon {
    font-size: 18px;
    color: var(--wf-primary);
  }

  &__count {
    font-size: 12px;
    font-weight: 400;
    color: var(--wf-text-muted);
  }

  &__desc {
    margin: 4px 0 0;
    font-size: 12px;
    color: var(--wf-text-muted);
    line-height: 1.5;
  }

  &__toolbar {
    display: flex;
    align-items: center;
    justify-content: flex-end;
    flex-wrap: wrap;
    gap: 10px;
    flex: 1;
    min-width: 320px;
    flex-shrink: 0;
  }

  &__group {
    display: inline-flex;
    align-items: center;
    flex-wrap: wrap;
    gap: 8px;
  }

  &__divider {
    width: 1px;
    height: 20px;
    background: #e4e7ed;
    flex-shrink: 0;
  }

  &__input {
    width: 148px;

    &--wide {
      width: 168px;
    }
  }
}

.wf-position-table {
  flex: 1;
  min-height: 0;
}
</style>
