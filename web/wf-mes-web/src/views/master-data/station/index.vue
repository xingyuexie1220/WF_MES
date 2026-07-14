<script setup lang="ts">
defineOptions({ name: 'MasterStation' })
import { onMounted, reactive, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { ElMessage, ElMessageBox } from 'element-plus'
import { CirclePlusFilled, Search, SetUp } from '@element-plus/icons-vue'
import { deleteMesProcessApi, getMesProcessesApi, saveMesProcessApi } from '@/api/master-data/mes'
import { WfDialogFooter, WfPage, WfPageBody, WfTable } from '@/components/page'
import type { MesProcessItem } from '@/types/master-data/mes'

const { t } = useI18n()
const loading = ref(false)
const submitting = ref(false)
const tableData = ref<MesProcessItem[]>([])
const dialogVisible = ref(false)
const editingId = ref<number | null>(null)
const keyword = ref('')
const form = reactive({
  processCode: '',
  processName: '',
  defaultSeq: 10,
  enabled: true,
  remark: ''
})

async function loadData() {
  loading.value = true
  try {
    const list = await getMesProcessesApi()
    const kw = keyword.value.trim().toLowerCase()
    tableData.value = kw
      ? list.filter(
          (x) =>
            x.processCode.toLowerCase().includes(kw) || x.processName.toLowerCase().includes(kw)
        )
      : list
  } finally {
    loading.value = false
  }
}

function openCreate() {
  editingId.value = null
  Object.assign(form, { processCode: '', processName: '', defaultSeq: 10, enabled: true, remark: '' })
  dialogVisible.value = true
}

function openEdit(row: MesProcessItem) {
  editingId.value = row.id
  Object.assign(form, row)
  dialogVisible.value = true
}

async function submitForm() {
  if (!form.processCode.trim() || !form.processName.trim()) {
    ElMessage.warning(t('mes.process.validateRequired'))
    return
  }
  submitting.value = true
  try {
    await saveMesProcessApi({ id: editingId.value || 0, ...form })
    ElMessage.success(editingId.value ? t('common.updateSuccess') : t('common.createSuccess'))
    dialogVisible.value = false
    await loadData()
  } finally {
    submitting.value = false
  }
}

async function handleDelete(row: MesProcessItem) {
  await ElMessageBox.confirm(t('mes.process.confirmDelete', { name: row.processName }), t('common.tip'), {
    type: 'warning'
  })
  await deleteMesProcessApi(row.id)
  ElMessage.success(t('common.deleteSuccess'))
  await loadData()
}

onMounted(loadData)
</script>

<template>
  <WfPage>
    <WfPageBody>
      <div class="wf-list-panel">
        <div class="wf-list-panel__head">
          <div class="wf-list-panel__title-block">
            <div class="wf-list-panel__title">
              <el-icon class="wf-list-panel__title-icon"><SetUp /></el-icon>
              {{ t('mes.process.title') }}
            </div>
            <p class="wf-list-panel__desc">{{ t('mes.process.pageDesc') }}</p>
          </div>
          <div class="wf-list-panel__toolbar">
            <el-input
              v-model="keyword"
              clearable
              :placeholder="t('mes.process.search')"
              class="wf-list-panel__input wf-list-panel__input--wide"
              @keyup.enter="loadData"
            />
            <el-button type="primary" :icon="Search" @click="loadData">{{ t('common.query') }}</el-button>
            <el-button type="success" :icon="CirclePlusFilled" @click="openCreate">{{ t('common.add') }}</el-button>
          </div>
        </div>

        <WfTable v-loading="loading" :data="tableData">
          <el-table-column prop="processCode" :label="t('mes.process.code')" min-width="120" />
          <el-table-column prop="processName" :label="t('mes.process.name')" min-width="140" />
          <el-table-column prop="defaultSeq" :label="t('common.sort')" width="90" align="center" />
          <el-table-column prop="enabled" :label="t('common.status')" width="100" align="center">
            <template #default="{ row }">
              <el-tag :type="row.enabled ? 'success' : 'info'" size="small">
                {{ row.enabled ? t('common.enabled') : t('common.disabled') }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column :label="t('common.actions')" width="160" fixed="right" align="center">
            <template #default="{ row }">
              <el-button link type="primary" @click="openEdit(row)">{{ t('common.edit') }}</el-button>
              <el-button link type="danger" @click="handleDelete(row)">{{ t('common.delete') }}</el-button>
            </template>
          </el-table-column>
        </WfTable>
      </div>
    </WfPageBody>

    <el-dialog
      v-model="dialogVisible"
      :title="editingId ? t('common.edit') : t('common.add')"
      width="520px"
      destroy-on-close
      align-center
    >
      <el-form :model="form" label-width="96px">
        <el-form-item :label="t('mes.process.code')" required>
          <el-input v-model="form.processCode" :disabled="!!editingId" />
        </el-form-item>
        <el-form-item :label="t('mes.process.name')" required>
          <el-input v-model="form.processName" />
        </el-form-item>
        <el-form-item :label="t('common.sort')">
          <el-input-number v-model="form.defaultSeq" :min="1" :max="999" />
        </el-form-item>
        <el-form-item :label="t('common.status')">
          <el-switch v-model="form.enabled" />
        </el-form-item>
        <el-form-item :label="t('common.remark')">
          <el-input v-model="form.remark" type="textarea" :rows="2" />
        </el-form-item>
      </el-form>
      <template #footer>
        <WfDialogFooter :loading="submitting" @cancel="dialogVisible = false" @confirm="submitForm" />
      </template>
    </el-dialog>
  </WfPage>
</template>
