<script setup lang="ts">
defineOptions({ name: 'MasterMaterial' })
import { onMounted, reactive, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Box, CirclePlusFilled, Search } from '@element-plus/icons-vue'
import { deleteMesMaterialApi, getMesMaterialsApi, saveMesMaterialApi } from '@/api/master-data/mes'
import { WfDialogFooter, WfPage, WfPageBody, WfTable } from '@/components/page'
import type { MesMaterialItem } from '@/types/master-data/mes'

const { t } = useI18n()
const loading = ref(false)
const submitting = ref(false)
const tableData = ref<MesMaterialItem[]>([])
const dialogVisible = ref(false)
const editingId = ref<number | null>(null)
const keyword = ref('')
const form = reactive({
  materialNo: '',
  materialName: '',
  spec: '',
  unit: 'PCS',
  enabled: true
})

async function loadData() {
  loading.value = true
  try {
    const list = await getMesMaterialsApi()
    const kw = keyword.value.trim().toLowerCase()
    tableData.value = kw
      ? list.filter(
          (x) =>
            x.materialNo.toLowerCase().includes(kw) || x.materialName.toLowerCase().includes(kw)
        )
      : list
  } finally {
    loading.value = false
  }
}

function openCreate() {
  editingId.value = null
  Object.assign(form, { materialNo: '', materialName: '', spec: '', unit: 'PCS', enabled: true })
  dialogVisible.value = true
}

function openEdit(row: MesMaterialItem) {
  editingId.value = row.id
  Object.assign(form, {
    materialNo: row.materialNo,
    materialName: row.materialName,
    spec: row.spec || '',
    unit: row.unit || 'PCS',
    enabled: row.enabled
  })
  dialogVisible.value = true
}

async function submitForm() {
  if (!form.materialNo.trim() || !form.materialName.trim()) {
    ElMessage.warning(t('mes.material.validateRequired'))
    return
  }
  submitting.value = true
  try {
    await saveMesMaterialApi({ id: editingId.value || 0, ...form })
    ElMessage.success(editingId.value ? t('common.updateSuccess') : t('common.createSuccess'))
    dialogVisible.value = false
    await loadData()
  } finally {
    submitting.value = false
  }
}

async function handleDelete(row: MesMaterialItem) {
  await ElMessageBox.confirm(t('mes.material.confirmDelete', { name: row.materialName }), t('common.tip'), {
    type: 'warning'
  })
  await deleteMesMaterialApi(row.id)
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
              <el-icon class="wf-list-panel__title-icon"><Box /></el-icon>
              {{ t('mes.material.title') }}
            </div>
            <p class="wf-list-panel__desc">{{ t('mes.material.pageDesc') }}</p>
          </div>
          <div class="wf-list-panel__toolbar">
            <el-input v-model="keyword" clearable :placeholder="t('mes.material.search')" @keyup.enter="loadData" />
            <el-button type="primary" :icon="Search" @click="loadData">{{ t('common.query') }}</el-button>
            <el-button type="success" :icon="CirclePlusFilled" @click="openCreate">{{ t('common.add') }}</el-button>
          </div>
        </div>
        <WfTable v-loading="loading" :data="tableData">
          <el-table-column prop="materialNo" :label="t('mes.material.code')" min-width="140" />
          <el-table-column prop="materialName" :label="t('mes.material.name')" min-width="180" />
          <el-table-column prop="spec" :label="t('mes.material.spec')" min-width="120" />
          <el-table-column prop="unit" :label="t('mes.material.unit')" width="80" />
          <el-table-column prop="source" :label="t('mes.material.source')" width="90" />
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
        <el-form-item :label="t('mes.material.code')" required>
          <el-input v-model="form.materialNo" :disabled="!!editingId" />
        </el-form-item>
        <el-form-item :label="t('mes.material.name')" required>
          <el-input v-model="form.materialName" />
        </el-form-item>
        <el-form-item :label="t('mes.material.spec')"><el-input v-model="form.spec" /></el-form-item>
        <el-form-item :label="t('mes.material.unit')"><el-input v-model="form.unit" /></el-form-item>
        <el-form-item :label="t('common.status')"><el-switch v-model="form.enabled" /></el-form-item>
      </el-form>
      <template #footer>
        <WfDialogFooter :loading="submitting" @cancel="dialogVisible = false" @confirm="submitForm" />
      </template>
    </el-dialog>
  </WfPage>
</template>
