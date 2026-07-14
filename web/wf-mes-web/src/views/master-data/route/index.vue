<script setup lang="ts">
defineOptions({ name: 'MasterRoute' })
import { onMounted, reactive, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { ElMessage, ElMessageBox } from 'element-plus'
import { CirclePlusFilled, Search, Share } from '@element-plus/icons-vue'
import {
  deleteMesRoutingApi,
  getMesProcessesApi,
  getMesRoutingsApi,
  saveMesRoutingApi
} from '@/api/master-data/mes'
import { WfDialogFooter, WfPage, WfPageBody, WfTable } from '@/components/page'
import type { MesProcessItem, MesRoutingItem, MesRoutingStep } from '@/types/master-data/mes'

const { t } = useI18n()
const loading = ref(false)
const submitting = ref(false)
const tableData = ref<MesRoutingItem[]>([])
const processes = ref<MesProcessItem[]>([])
const dialogVisible = ref(false)
const editingId = ref<number | null>(null)
const form = reactive({
  routingCode: '',
  routingName: '',
  materialNo: '',
  enabled: true,
  remark: '',
  steps: [] as MesRoutingStep[]
})

async function loadData() {
  loading.value = true
  try {
    tableData.value = await getMesRoutingsApi()
  } finally {
    loading.value = false
  }
}

async function loadProcesses() {
  processes.value = await getMesProcessesApi()
}

function openCreate() {
  editingId.value = null
  Object.assign(form, {
    routingCode: '',
    routingName: '',
    materialNo: '',
    enabled: true,
    remark: '',
    steps: processes.value.map((p) => ({
      processCode: p.processCode,
      processName: p.processName,
      seq: p.defaultSeq
    }))
  })
  dialogVisible.value = true
}

function openEdit(row: MesRoutingItem) {
  editingId.value = row.id
  Object.assign(form, {
    routingCode: row.routingCode,
    routingName: row.routingName,
    materialNo: row.materialNo || '',
    enabled: row.enabled,
    remark: row.remark || '',
    steps: row.steps.map((s) => ({ ...s }))
  })
  dialogVisible.value = true
}

function addStep() {
  const first = processes.value[0]
  form.steps.push({
    processCode: first?.processCode || '',
    processName: first?.processName,
    seq: (form.steps.at(-1)?.seq || 0) + 10
  })
}

function removeStep(index: number) {
  form.steps.splice(index, 1)
}

function onProcessChange(step: MesRoutingStep) {
  const p = processes.value.find((x) => x.processCode === step.processCode)
  step.processName = p?.processName
}

async function submitForm() {
  if (!form.routingCode.trim() || !form.routingName.trim()) {
    ElMessage.warning(t('mes.routing.validateRequired'))
    return
  }
  if (!form.steps.length) {
    ElMessage.warning(t('mes.routing.needSteps'))
    return
  }
  submitting.value = true
  try {
    await saveMesRoutingApi({ id: editingId.value || 0, ...form })
    ElMessage.success(editingId.value ? t('common.updateSuccess') : t('common.createSuccess'))
    dialogVisible.value = false
    await loadData()
  } finally {
    submitting.value = false
  }
}

async function handleDelete(row: MesRoutingItem) {
  await ElMessageBox.confirm(t('mes.routing.confirmDelete', { name: row.routingName }), t('common.tip'), {
    type: 'warning'
  })
  await deleteMesRoutingApi(row.id)
  ElMessage.success(t('common.deleteSuccess'))
  await loadData()
}

function stepsText(row: MesRoutingItem) {
  return row.steps.map((s) => s.processCode).join(' → ')
}

onMounted(async () => {
  await loadProcesses()
  await loadData()
})
</script>

<template>
  <WfPage>
    <WfPageBody>
      <div class="wf-list-panel">
        <div class="wf-list-panel__head">
          <div class="wf-list-panel__title-block">
            <div class="wf-list-panel__title">
              <el-icon class="wf-list-panel__title-icon"><Share /></el-icon>
              {{ t('mes.routing.title') }}
            </div>
            <p class="wf-list-panel__desc">{{ t('mes.routing.pageDesc') }}</p>
          </div>
          <div class="wf-list-panel__toolbar">
            <el-button type="primary" :icon="Search" @click="loadData">{{ t('common.query') }}</el-button>
            <el-button type="success" :icon="CirclePlusFilled" @click="openCreate">{{ t('common.add') }}</el-button>
          </div>
        </div>
        <WfTable v-loading="loading" :data="tableData">
          <el-table-column prop="routingCode" :label="t('mes.routing.code')" min-width="120" />
          <el-table-column prop="routingName" :label="t('mes.routing.name')" min-width="160" />
          <el-table-column prop="materialNo" :label="t('mes.material.code')" min-width="140" />
          <el-table-column :label="t('mes.routing.steps')" min-width="260">
            <template #default="{ row }">{{ stepsText(row) }}</template>
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

    <el-dialog v-model="dialogVisible" :title="editingId ? t('common.edit') : t('common.add')" width="720px" align-center>
      <el-form :model="form" label-width="96px">
        <el-form-item :label="t('mes.routing.code')" required>
          <el-input v-model="form.routingCode" :disabled="!!editingId" />
        </el-form-item>
        <el-form-item :label="t('mes.routing.name')" required>
          <el-input v-model="form.routingName" />
        </el-form-item>
        <el-form-item :label="t('mes.material.code')">
          <el-input v-model="form.materialNo" :placeholder="t('mes.routing.materialOptional')" />
        </el-form-item>
        <el-form-item :label="t('mes.routing.steps')">
          <div class="steps">
            <div v-for="(step, index) in form.steps" :key="index" class="steps__row">
              <el-input-number v-model="step.seq" :min="1" :max="999" />
              <el-select v-model="step.processCode" style="width: 220px" @change="onProcessChange(step)">
                <el-option
                  v-for="p in processes"
                  :key="p.processCode"
                  :label="`${p.processCode} ${p.processName}`"
                  :value="p.processCode"
                />
              </el-select>
              <el-button link type="danger" @click="removeStep(index)">{{ t('common.delete') }}</el-button>
            </div>
            <el-button @click="addStep">{{ t('mes.routing.addStep') }}</el-button>
          </div>
        </el-form-item>
      </el-form>
      <template #footer>
        <WfDialogFooter :loading="submitting" @cancel="dialogVisible = false" @confirm="submitForm" />
      </template>
    </el-dialog>
  </WfPage>
</template>

<style scoped>
.steps {
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: 8px;
}
.steps__row {
  display: flex;
  gap: 8px;
  align-items: center;
}
</style>
