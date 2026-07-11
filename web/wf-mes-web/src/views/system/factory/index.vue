<script setup lang="ts">
defineOptions({ name: 'SystemFactory' })
import { computed, onMounted, reactive, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { ElMessage, ElMessageBox } from 'element-plus'
import { CirclePlusFilled } from '@element-plus/icons-vue'
import { useUserStore } from '@/stores/auth/user'
import {
  createFactoryApi,
  deleteFactoryApi,
  getFactoryListApi,
  getFactoryRegionsApi,
  updateFactoryApi
} from '@/api/system/factory'
import { WfDialogFooter, WfPage, WfPageBody, WfStatusTag, WfTable } from '@/components/page'
import type { FactoryItem, RegionItem } from '@/types/common/factory'

const { t } = useI18n()
const userStore = useUserStore()
const loading = ref(false)
const submitting = ref(false)
const tableData = ref<FactoryItem[]>([])
const regions = ref<RegionItem[]>([])
const dialogVisible = ref(false)
const editingId = ref<number | null>(null)

const form = reactive({
  regionId: 0,
  factoryCode: '',
  factoryName: '',
  timeZone: 'Asia/Shanghai',
  sort: 1,
  status: 1,
  remark: ''
})

const canAdd = computed(() => userStore.hasPermission('system:factory:add'))
const canEdit = computed(() => userStore.hasPermission('system:factory:edit'))
const canDelete = computed(() => userStore.hasPermission('system:factory:delete'))
const dialogTitle = computed(() => (editingId.value ? t('factory.edit') : t('factory.add')))

async function loadData() {
  loading.value = true
  try {
    const [list, regionList] = await Promise.all([getFactoryListApi(), getFactoryRegionsApi()])
    tableData.value = list
    regions.value = regionList
  } finally {
    loading.value = false
  }
}

function resetForm() {
  form.regionId = regions.value[0]?.id ?? 0
  form.factoryCode = ''
  form.factoryName = ''
  form.timeZone = 'Asia/Shanghai'
  form.sort = 1
  form.status = 1
  form.remark = ''
}

function openCreate() {
  editingId.value = null
  resetForm()
  dialogVisible.value = true
}

function openEdit(row: FactoryItem) {
  editingId.value = row.id ?? null
  form.regionId = row.regionId
  form.factoryCode = row.factoryCode
  form.factoryName = row.factoryName
  form.timeZone = row.timeZone || 'Asia/Shanghai'
  form.sort = row.sort
  form.status = row.status
  form.remark = row.remark || ''
  dialogVisible.value = true
}

async function submitForm() {
  if (!form.factoryCode.trim() || !form.factoryName.trim() || !form.regionId) {
    ElMessage.warning(t('factory.validateRequired'))
    return
  }
  submitting.value = true
  try {
    if (editingId.value) {
      await updateFactoryApi(editingId.value, { ...form })
      ElMessage.success(t('common.updateSuccess'))
    } else {
      await createFactoryApi({ ...form })
      ElMessage.success(t('common.createSuccess'))
    }
    dialogVisible.value = false
    await loadData()
  } finally {
    submitting.value = false
  }
}

async function handleDelete(row: FactoryItem) {
  await ElMessageBox.confirm(t('factory.confirmDelete', { name: row.factoryName }), t('common.tip'), {
    type: 'warning'
  })
  await deleteFactoryApi(row.id!)
  ElMessage.success(t('common.deleteSuccess'))
  await loadData()
}

onMounted(loadData)
</script>

<template>
  <WfPage>
    <template #title>{{ t('factory.title') }}</template>
    <template #desc>{{ t('factory.pageDesc') }}</template>
    <template #actions>
      <el-button v-if="canAdd" type="primary" :icon="CirclePlusFilled" @click="openCreate">
        {{ t('factory.add') }}
      </el-button>
    </template>

    <WfPageBody>
      <WfTable v-loading="loading" :data="tableData" row-key="id">
        <el-table-column prop="factoryCode" :label="t('factory.code')" min-width="120" />
        <el-table-column prop="factoryName" :label="t('factory.name')" min-width="160" />
        <el-table-column prop="regionName" :label="t('factory.region')" min-width="120" />
        <el-table-column prop="timeZone" :label="t('factory.timeZone')" min-width="140" />
        <el-table-column prop="sort" :label="t('common.sort')" width="80" align="center" />
        <el-table-column :label="t('common.status')" width="90" align="center">
          <template #default="{ row }">
            <WfStatusTag :status="row.status" />
          </template>
        </el-table-column>
        <el-table-column :label="t('common.actions')" width="160" fixed="right">
          <template #default="{ row }">
            <el-button v-if="canEdit" link type="primary" @click="openEdit(row)">{{ t('common.edit') }}</el-button>
            <el-button v-if="canDelete" link type="danger" @click="handleDelete(row)">{{ t('common.delete') }}</el-button>
          </template>
        </el-table-column>
      </WfTable>
    </WfPageBody>
  </WfPage>

  <el-dialog v-model="dialogVisible" :title="dialogTitle" width="520px" destroy-on-close>
    <el-form label-width="100px">
      <el-form-item :label="t('factory.region')" required>
        <el-select v-model="form.regionId" style="width: 100%">
          <el-option v-for="item in regions" :key="item.id" :label="item.regionName" :value="item.id" />
        </el-select>
      </el-form-item>
      <el-form-item :label="t('factory.code')" required>
        <el-input v-model.trim="form.factoryCode" />
      </el-form-item>
      <el-form-item :label="t('factory.name')" required>
        <el-input v-model.trim="form.factoryName" />
      </el-form-item>
      <el-form-item :label="t('factory.timeZone')">
        <el-input v-model.trim="form.timeZone" />
      </el-form-item>
      <el-form-item :label="t('common.sort')">
        <el-input-number v-model="form.sort" :min="0" />
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
</template>
