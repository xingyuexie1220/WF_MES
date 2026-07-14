<script setup lang="ts">
defineOptions({ name: 'MasterWorkCenter' })
import { onMounted, reactive, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { ElMessage, ElMessageBox } from 'element-plus'
import { CirclePlusFilled, OfficeBuilding, Search } from '@element-plus/icons-vue'
import { deleteMesMachineApi, getMesMachinesApi, saveMesMachineApi } from '@/api/master-data/mes'
import { WfDialogFooter, WfPage, WfPageBody, WfTable } from '@/components/page'
import type { MesMachineItem } from '@/types/master-data/mes'

const { t } = useI18n()
const loading = ref(false)
const submitting = ref(false)
const tableData = ref<MesMachineItem[]>([])
const dialogVisible = ref(false)
const editingId = ref<number | null>(null)
const keyword = ref('')
const form = reactive({ machineNo: '', machineName: '', enabled: true, remark: '' })

async function loadData() {
  loading.value = true
  try {
    const list = await getMesMachinesApi()
    const kw = keyword.value.trim().toLowerCase()
    tableData.value = kw
      ? list.filter((x) => x.machineNo.toLowerCase().includes(kw) || x.machineName.toLowerCase().includes(kw))
      : list
  } finally {
    loading.value = false
  }
}

function openCreate() {
  editingId.value = null
  Object.assign(form, { machineNo: '', machineName: '', enabled: true, remark: '' })
  dialogVisible.value = true
}

function openEdit(row: MesMachineItem) {
  editingId.value = row.id
  Object.assign(form, row)
  dialogVisible.value = true
}

async function submitForm() {
  if (!form.machineNo.trim() || !form.machineName.trim()) {
    ElMessage.warning(t('mes.machine.validateRequired'))
    return
  }
  submitting.value = true
  try {
    await saveMesMachineApi({ id: editingId.value || 0, ...form })
    ElMessage.success(editingId.value ? t('common.updateSuccess') : t('common.createSuccess'))
    dialogVisible.value = false
    await loadData()
  } finally {
    submitting.value = false
  }
}

async function handleDelete(row: MesMachineItem) {
  await ElMessageBox.confirm(t('mes.machine.confirmDelete', { name: row.machineName }), t('common.tip'), {
    type: 'warning'
  })
  await deleteMesMachineApi(row.id)
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
              <el-icon class="wf-list-panel__title-icon"><OfficeBuilding /></el-icon>
              {{ t('mes.machine.title') }}
            </div>
            <p class="wf-list-panel__desc">{{ t('mes.machine.pageDesc') }}</p>
          </div>
          <div class="wf-list-panel__toolbar">
            <el-input v-model="keyword" clearable :placeholder="t('mes.machine.search')" @keyup.enter="loadData" />
            <el-button type="primary" :icon="Search" @click="loadData">{{ t('common.query') }}</el-button>
            <el-button type="success" :icon="CirclePlusFilled" @click="openCreate">{{ t('common.add') }}</el-button>
          </div>
        </div>
        <WfTable v-loading="loading" :data="tableData">
          <el-table-column prop="machineNo" :label="t('mes.machine.code')" min-width="120" />
          <el-table-column prop="machineName" :label="t('mes.machine.name')" min-width="160" />
          <el-table-column :label="t('common.actions')" width="160" fixed="right" align="center">
            <template #default="{ row }">
              <el-button link type="primary" @click="openEdit(row)">{{ t('common.edit') }}</el-button>
              <el-button link type="danger" @click="handleDelete(row)">{{ t('common.delete') }}</el-button>
            </template>
          </el-table-column>
        </WfTable>
      </div>
    </WfPageBody>
    <el-dialog v-model="dialogVisible" :title="editingId ? t('common.edit') : t('common.add')" width="520px" align-center>
      <el-form :model="form" label-width="96px">
        <el-form-item :label="t('mes.machine.code')" required>
          <el-input v-model="form.machineNo" :disabled="!!editingId" />
        </el-form-item>
        <el-form-item :label="t('mes.machine.name')" required>
          <el-input v-model="form.machineName" />
        </el-form-item>
        <el-form-item :label="t('common.status')"><el-switch v-model="form.enabled" /></el-form-item>
        <el-form-item :label="t('common.remark')"><el-input v-model="form.remark" type="textarea" :rows="2" /></el-form-item>
      </el-form>
      <template #footer>
        <WfDialogFooter :loading="submitting" @cancel="dialogVisible = false" @confirm="submitForm" />
      </template>
    </el-dialog>
  </WfPage>
</template>
