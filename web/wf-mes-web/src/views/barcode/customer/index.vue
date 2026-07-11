<script setup lang="ts">
defineOptions({ name: 'BarcodeCustomer' })
import { computed, onMounted, reactive, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { ElMessage, ElMessageBox } from 'element-plus'
import { CirclePlusFilled, Printer } from '@element-plus/icons-vue'
import { useUserStore } from '@/stores/auth/user'
import { useBarcodeStore } from '@/stores/barcode/barcode'
import WfBarcodePreview from '@/components/barcode/WfBarcodePreview.vue'
import { WfDialogFooter, WfPage, WfPageBody, WfStatusTag, WfVxeTable } from '@/components/page'
import { formatDateTime } from '@/utils/format/date'
import { printLabelHtml } from '@/utils/print/label'
import type { BarcodeCustomerDto } from '@/types/barcode/customer'

const { t } = useI18n()
const userStore = useUserStore()
const barcodeStore = useBarcodeStore()
const dialogVisible = ref(false)
const editingId = ref(0)
const previewCode = ref('WF-MES-DEMO')

const form = reactive({
  customerName: '',
  enable: 1
})

const canEdit = computed(() => userStore.hasPermission('barcode:customer:list'))
const dialogTitle = computed(() =>
  editingId.value ? t('barcode.customer.edit') : t('barcode.customer.add')
)

function openCreate() {
  editingId.value = 0
  form.customerName = ''
  form.enable = 1
  dialogVisible.value = true
}

function openEdit(row: BarcodeCustomerDto) {
  editingId.value = row.customerId
  form.customerName = row.customerName
  form.enable = row.enable
  dialogVisible.value = true
}

async function loadData() {
  await barcodeStore.fetchCustomers(true)
}

async function submitForm() {
  if (!form.customerName.trim()) {
    ElMessage.warning(t('barcode.customer.validateName'))
    return
  }
  await barcodeStore.saveCustomer({
    customerId: editingId.value,
    customerName: form.customerName.trim(),
    enable: form.enable
  })
  ElMessage.success(editingId.value ? t('common.updateSuccess') : t('common.createSuccess'))
  dialogVisible.value = false
}

async function handleDelete(row: BarcodeCustomerDto) {
  await ElMessageBox.confirm(
    t('barcode.customer.confirmDelete', { name: row.customerName }),
    t('common.tip'),
    { type: 'warning' }
  )
  await barcodeStore.saveCustomer({
    customerId: row.customerId,
    customerName: row.customerName,
    enable: 0
  })
  ElMessage.success(t('common.deleteSuccess'))
}

function handlePrintPreview() {
  const html = document.getElementById('wf-barcode-print-area')?.innerHTML
  if (!html) {
    return
  }
  printLabelHtml(html, t('barcode.customer.title'))
}

onMounted(loadData)
</script>

<template>
  <WfPage>
    <template #title>{{ t('barcode.customer.title') }}</template>
    <template #desc>{{ t('barcode.customer.pageDesc') }}</template>
    <template #actions>
      <el-button v-if="canEdit" type="primary" :icon="CirclePlusFilled" @click="openCreate">
        {{ t('barcode.customer.add') }}
      </el-button>
      <el-button :icon="Printer" @click="handlePrintPreview">{{ t('common.export') }}</el-button>
    </template>

    <WfPageBody>
      <div id="wf-barcode-print-area" class="barcode-customer__preview">
        <WfBarcodePreview :value="previewCode" />
      </div>

      <WfVxeTable
        v-loading="barcodeStore.customersLoading"
        :data="barcodeStore.customers"
        row-id="customerId"
      >
        <vxe-column field="customerId" title="ID" width="80" />
        <vxe-column field="customerName" :title="t('barcode.customer.customerName')" min-width="160" />
        <vxe-column field="enable" :title="t('barcode.customer.enable')" width="100" align="center">
          <template #default="{ row }">
            <WfStatusTag :status="row.enable" />
          </template>
        </vxe-column>
        <vxe-column field="createDate" :title="t('common.createTime')" min-width="168">
          <template #default="{ row }">
            {{ formatDateTime(row.createDate) }}
          </template>
        </vxe-column>
        <vxe-column :title="t('common.actions')" width="160" fixed="right">
          <template #default="{ row }">
            <el-button v-if="canEdit" link type="primary" @click="openEdit(row)">
              {{ t('common.edit') }}
            </el-button>
            <el-button v-if="canEdit" link type="danger" @click="handleDelete(row)">
              {{ t('common.delete') }}
            </el-button>
          </template>
        </vxe-column>
        <template #empty>
          <el-empty :description="t('barcode.customer.empty')" />
        </template>
      </WfVxeTable>
    </WfPageBody>
  </WfPage>

  <el-dialog v-model="dialogVisible" :title="dialogTitle" width="480px" destroy-on-close>
    <el-form label-width="100px">
      <el-form-item :label="t('barcode.customer.customerName')" required>
        <el-input v-model.trim="form.customerName" />
      </el-form-item>
      <el-form-item :label="t('barcode.customer.enable')">
        <el-radio-group v-model="form.enable">
          <el-radio :value="1">{{ t('common.enabled') }}</el-radio>
          <el-radio :value="0">{{ t('common.disabled') }}</el-radio>
        </el-radio-group>
      </el-form-item>
    </el-form>
    <template #footer>
      <WfDialogFooter @cancel="dialogVisible = false" @confirm="submitForm" />
    </template>
  </el-dialog>
</template>

<style scoped lang="scss">
.barcode-customer__preview {
  margin-bottom: 16px;
  padding: 12px;
  border: 1px dashed var(--wf-border);
  border-radius: 8px;
  background: var(--wf-content-bg);
}
</style>
