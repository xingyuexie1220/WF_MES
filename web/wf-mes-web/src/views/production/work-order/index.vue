<script setup lang="ts">
defineOptions({ name: 'ProductionWorkOrder' })
import { onMounted, reactive, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { ElMessage, ElMessageBox } from 'element-plus'
import { CirclePlusFilled, Document, Search } from '@element-plus/icons-vue'
import { getMesMaterialsApi, getMesRoutingsApi } from '@/api/master-data/mes'
import { closeWorkOrderApi, getWorkOrdersApi, saveWorkOrderApi } from '@/api/production/work-order'
import { WfDialogFooter, WfPage, WfPageBody, WfTable } from '@/components/page'
import type { MesMaterialItem, MesRoutingItem } from '@/types/master-data/mes'
import type { WorkOrderDto } from '@/types/production/work-order'

const { t } = useI18n()
const loading = ref(false)
const submitting = ref(false)
const tableData = ref<WorkOrderDto[]>([])
const materials = ref<MesMaterialItem[]>([])
const routings = ref<MesRoutingItem[]>([])
const dialogVisible = ref(false)
const form = reactive({
  workOrderNo: '',
  materialNo: '',
  routingId: undefined as number | undefined,
  planQty: 100,
  dueDate: '',
  remark: ''
})

async function loadData() {
  loading.value = true
  try {
    tableData.value = await getWorkOrdersApi()
  } finally {
    loading.value = false
  }
}

async function loadOptions() {
  ;[materials.value, routings.value] = await Promise.all([getMesMaterialsApi(), getMesRoutingsApi()])
}

function openCreate() {
  const today = new Date()
  const no = `WO${today.getFullYear()}${String(today.getMonth() + 1).padStart(2, '0')}${String(today.getDate()).padStart(2, '0')}${String(today.getHours()).padStart(2, '0')}${String(today.getMinutes()).padStart(2, '0')}`
  Object.assign(form, {
    workOrderNo: no,
    materialNo: materials.value[0]?.materialNo || '',
    routingId: routings.value[0]?.id,
    planQty: 100,
    dueDate: '',
    remark: ''
  })
  dialogVisible.value = true
}

async function submitForm() {
  if (!form.workOrderNo.trim() || !form.materialNo.trim() || form.planQty <= 0) {
    ElMessage.warning(t('mes.workOrder.validateRequired'))
    return
  }
  submitting.value = true
  try {
    await saveWorkOrderApi({
      workOrderNo: form.workOrderNo,
      materialNo: form.materialNo,
      routingId: form.routingId,
      planQty: form.planQty,
      dueDate: form.dueDate || undefined,
      remark: form.remark
    })
    ElMessage.success(t('common.createSuccess'))
    dialogVisible.value = false
    await loadData()
  } finally {
    submitting.value = false
  }
}

async function handleClose(row: WorkOrderDto) {
  await ElMessageBox.confirm(t('mes.workOrder.confirmClose', { no: row.workOrderNo }), t('common.tip'), {
    type: 'warning'
  })
  await closeWorkOrderApi(row.workOrderId)
  ElMessage.success(t('common.success'))
  await loadData()
}

function progressText(row: WorkOrderDto) {
  return (row.progress || [])
    .map((p) => `${p.processCode}:${p.goodQty}/${p.goodQty + p.defectQty + p.remainQty}`)
    .join(' · ')
}

onMounted(async () => {
  await loadOptions()
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
              <el-icon class="wf-list-panel__title-icon"><Document /></el-icon>
              {{ t('mes.workOrder.title') }}
            </div>
            <p class="wf-list-panel__desc">{{ t('mes.workOrder.pageDesc') }}</p>
          </div>
          <div class="wf-list-panel__toolbar">
            <el-button type="primary" :icon="Search" @click="loadData">{{ t('common.query') }}</el-button>
            <el-button type="success" :icon="CirclePlusFilled" @click="openCreate">{{ t('common.add') }}</el-button>
          </div>
        </div>
        <WfTable v-loading="loading" :data="tableData">
          <el-table-column prop="workOrderNo" :label="t('mes.workOrder.no')" min-width="150" />
          <el-table-column prop="materialNo" :label="t('mes.material.code')" min-width="130" />
          <el-table-column prop="materialName" :label="t('mes.material.name')" min-width="160" />
          <el-table-column prop="planQty" :label="t('mes.workOrder.planQty')" width="90" align="center" />
          <el-table-column prop="status" :label="t('common.status')" width="90" align="center">
            <template #default="{ row }">
              <el-tag :type="row.status === 'open' ? 'success' : 'info'" size="small">{{ row.status }}</el-tag>
            </template>
          </el-table-column>
          <el-table-column :label="t('mes.workOrder.progress')" min-width="280" show-overflow-tooltip>
            <template #default="{ row }">{{ progressText(row) }}</template>
          </el-table-column>
          <el-table-column :label="t('common.actions')" width="120" fixed="right" align="center">
            <template #default="{ row }">
              <el-button
                v-if="row.status === 'open'"
                link
                type="warning"
                @click="handleClose(row)"
              >
                {{ t('mes.workOrder.close') }}
              </el-button>
            </template>
          </el-table-column>
        </WfTable>
      </div>
    </WfPageBody>

    <el-dialog v-model="dialogVisible" :title="t('mes.workOrder.add')" width="560px" align-center>
      <el-form :model="form" label-width="96px">
        <el-form-item :label="t('mes.workOrder.no')" required>
          <el-input v-model="form.workOrderNo" />
        </el-form-item>
        <el-form-item :label="t('mes.material.code')" required>
          <el-select v-model="form.materialNo" filterable style="width: 100%">
            <el-option
              v-for="m in materials"
              :key="m.materialNo"
              :label="`${m.materialNo} ${m.materialName}`"
              :value="m.materialNo"
            />
          </el-select>
        </el-form-item>
        <el-form-item :label="t('mes.routing.name')">
          <el-select v-model="form.routingId" clearable style="width: 100%">
            <el-option v-for="r in routings" :key="r.id" :label="`${r.routingCode} ${r.routingName}`" :value="r.id" />
          </el-select>
        </el-form-item>
        <el-form-item :label="t('mes.workOrder.planQty')" required>
          <el-input-number v-model="form.planQty" :min="1" :max="999999" />
        </el-form-item>
        <el-form-item :label="t('mes.workOrder.dueDate')">
          <el-date-picker v-model="form.dueDate" type="date" value-format="YYYY-MM-DD" style="width: 100%" />
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
