<script setup lang="ts">
defineOptions({ name: 'SystemRole' })
import { computed, onMounted, reactive, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { ElMessage, ElMessageBox } from 'element-plus'
import { CirclePlusFilled, Search, UserFilled } from '@element-plus/icons-vue'
import { getMenuTreeApi } from '@/api/system/menu'
import { createRoleApi, deleteRoleApi, getRolePageApi, updateRoleApi } from '@/api/system/role'
import {
  WfDialogFooter,
  WfMenuPermTree,
  WfPage,
  WfPageBody,
  WfPagePager,
  WfStatusTag,
  WfTable
} from '@/components/page'
import { CLIENT_DESKTOP, CLIENT_MOBILE, CLIENT_WEB } from '@/types/common/enums'
import type { MenuItem } from '@/types/system/menu'
import type { RoleItem } from '@/types/system/role'

const { t } = useI18n()
const loading = ref(false)
const submitting = ref(false)
const permSubmitting = ref(false)
const tableData = ref<RoleItem[]>([])
const total = ref(0)
const webMenuTree = ref<MenuItem[]>([])
const mobileMenuTree = ref<MenuItem[]>([])
const desktopMenuTree = ref<MenuItem[]>([])
const dialogVisible = ref(false)
const permDialogVisible = ref(false)
const editingId = ref<number | null>(null)
const assigningRole = ref<RoleItem | null>(null)
const permTab = ref<'web' | 'mobile' | 'desktop'>('web')
const permMenuIds = ref<number[]>([])
const preservedMenuIds = ref<number[]>([])
const preservedDeptIds = ref<number[]>([])

const query = reactive({
  pageIndex: 1,
  pageSize: 20,
  roleName: '',
  status: undefined as number | undefined
})
const form = reactive({
  roleCode: '',
  roleName: '',
  sort: 1,
  dataScope: 3,
  status: 1,
  remark: ''
})

const webMenuIdSet = computed(() => new Set(collectMenuIds(webMenuTree.value)))
const mobileMenuIdSet = computed(() => new Set(collectMenuIds(mobileMenuTree.value)))
const desktopMenuIdSet = computed(() => new Set(collectMenuIds(desktopMenuTree.value)))

const webMenuIds = computed({
  get: () => permMenuIds.value.filter((id) => webMenuIdSet.value.has(id)),
  set: (ids: number[]) => {
    permMenuIds.value = [
      ...ids,
      ...permMenuIds.value.filter((id) => mobileMenuIdSet.value.has(id) || desktopMenuIdSet.value.has(id))
    ]
  }
})

const mobileMenuIds = computed({
  get: () => permMenuIds.value.filter((id) => mobileMenuIdSet.value.has(id)),
  set: (ids: number[]) => {
    permMenuIds.value = [
      ...permMenuIds.value.filter((id) => webMenuIdSet.value.has(id) || desktopMenuIdSet.value.has(id)),
      ...ids
    ]
  }
})

const desktopMenuIds = computed({
  get: () => permMenuIds.value.filter((id) => desktopMenuIdSet.value.has(id)),
  set: (ids: number[]) => {
    permMenuIds.value = [
      ...permMenuIds.value.filter((id) => webMenuIdSet.value.has(id) || mobileMenuIdSet.value.has(id)),
      ...ids
    ]
  }
})

const assignPermTitle = computed(() =>
  assigningRole.value
    ? t('system.role.assignMenuPermTitle', { name: assigningRole.value.roleName })
    : t('system.role.assignMenuPerm')
)

function formatUserName(name?: string | null, userId?: number) {
  if (name) return name
  if (userId) return String(userId)
  return t('common.none')
}

function formatDateShort(value?: string | null) {
  if (!value) return t('common.none')
  const date = new Date(value)
  if (Number.isNaN(date.getTime()) || date.getFullYear() < 1970) return t('common.none')
  const y = date.getFullYear()
  const m = String(date.getMonth() + 1).padStart(2, '0')
  const d = String(date.getDate()).padStart(2, '0')
  const hh = String(date.getHours()).padStart(2, '0')
  const mm = String(date.getMinutes()).padStart(2, '0')
  return `${y}-${m}-${d} ${hh}:${mm}`
}

function collectMenuIds(nodes: MenuItem[]): number[] {
  const ids: number[] = []
  const walk = (list: MenuItem[]) => {
    for (const node of list) {
      ids.push(node.id)
      if (node.children?.length) {
        walk(node.children)
      }
    }
  }
  walk(nodes)
  return ids
}

async function loadData() {
  loading.value = true
  try {
    const data = await getRolePageApi({ ...query })
    tableData.value = data.items
    total.value = data.total
  } finally {
    loading.value = false
  }
}

async function loadOptions() {
  const [webMenus, mobileMenus, desktopMenus] = await Promise.all([
    getMenuTreeApi(CLIENT_WEB),
    getMenuTreeApi(CLIENT_MOBILE),
    getMenuTreeApi(CLIENT_DESKTOP)
  ])
  webMenuTree.value = webMenus
  mobileMenuTree.value = mobileMenus
  desktopMenuTree.value = desktopMenus
}

function handleSearch() {
  query.pageIndex = 1
  loadData()
}

function handleReset() {
  query.roleName = ''
  query.status = undefined
  query.pageIndex = 1
  loadData()
}

function resetForm() {
  Object.assign(form, {
    roleCode: '',
    roleName: '',
    sort: 1,
    dataScope: 3,
    status: 1,
    remark: ''
  })
}

function openCreate() {
  editingId.value = null
  preservedMenuIds.value = []
  preservedDeptIds.value = []
  resetForm()
  dialogVisible.value = true
}

function openEdit(row: RoleItem) {
  editingId.value = row.id
  preservedMenuIds.value = [...(row.menuIds ?? [])]
  preservedDeptIds.value = [...(row.deptIds ?? [])]
  Object.assign(form, {
    roleCode: row.roleCode,
    roleName: row.roleName,
    sort: row.sort,
    dataScope: row.dataScope,
    status: row.status,
    remark: row.remark ?? ''
  })
  dialogVisible.value = true
}

function openAssignPerm(row: RoleItem) {
  assigningRole.value = row
  permMenuIds.value = [...(row.menuIds ?? [])]
  permTab.value = 'web'
  permDialogVisible.value = true
}

async function submitForm() {
  if (!form.roleCode.trim() || !form.roleName.trim()) {
    ElMessage.warning(t('system.role.formRequired'))
    return
  }

  submitting.value = true
  try {
    if (editingId.value) {
      await updateRoleApi(editingId.value, {
        ...form,
        menuIds: preservedMenuIds.value,
        deptIds: preservedDeptIds.value
      })
      ElMessage.success(t('common.updateSuccess'))
      dialogVisible.value = false
      await loadData()
    } else {
      const id = await createRoleApi({
        ...form,
        menuIds: [],
        deptIds: []
      })
      dialogVisible.value = false
      await loadData()
      ElMessage.success(t('system.role.createSuccess'))
      openAssignPerm({
        id,
        roleCode: form.roleCode,
        roleName: form.roleName,
        sort: form.sort,
        dataScope: form.dataScope,
        status: form.status,
        remark: form.remark,
        menuIds: [],
        deptIds: []
      })
    }
  } finally {
    submitting.value = false
  }
}

async function submitPerm() {
  if (!assigningRole.value) {
    return
  }

  permSubmitting.value = true
  try {
    const role = assigningRole.value
    await updateRoleApi(role.id, {
      roleName: role.roleName,
      sort: role.sort,
      dataScope: role.dataScope,
      status: role.status,
      remark: role.remark ?? '',
      menuIds: permMenuIds.value,
      deptIds: role.deptIds ?? []
    })
    ElMessage.success(t('system.role.assignSuccess'))
    permDialogVisible.value = false
    await loadData()
  } finally {
    permSubmitting.value = false
  }
}

async function handleDelete(row: RoleItem) {
  await ElMessageBox.confirm(t('system.role.confirmDelete', { name: row.roleName }), t('common.tip'), {
    type: 'warning'
  })
  await deleteRoleApi(row.id)
  ElMessage.success(t('common.deleteSuccess'))
  loadData()
}

onMounted(async () => {
  await loadOptions()
  await loadData()
})
</script>

<template>
  <WfPage class="wf-role-page">
    <WfPageBody class="wf-role-page__body">
      <div class="wf-list-panel">
        <div class="wf-list-panel__head">
          <div class="wf-list-panel__title-block">
            <div class="wf-list-panel__title">
              <el-icon class="wf-list-panel__title-icon"><UserFilled /></el-icon>
              <span>{{ t('system.role.title') }}</span>
              <span class="wf-list-panel__count">{{ t('system.role.roleTotal', { count: total }) }}</span>
            </div>
            <p class="wf-list-panel__desc">{{ t('system.role.pageDesc') }}</p>
          </div>
          <div class="wf-list-panel__toolbar">
            <div class="wf-list-panel__group wf-list-panel__group--filter">
              <el-input
                v-model="query.roleName"
                clearable
                :prefix-icon="Search"
                :placeholder="t('system.role.searchRoleName')"
                class="wf-list-panel__input wf-list-panel__input--wide"
                @keyup.enter="handleSearch"
              />
              <el-select
                v-model="query.status"
                clearable
                :placeholder="t('common.status')"
                class="wf-list-panel__select"
              >
                <el-option :label="t('common.enabled')" :value="1" />
                <el-option :label="t('common.disabled')" :value="0" />
              </el-select>
            </div>
            <span class="wf-list-panel__divider" aria-hidden="true" />
            <div class="wf-list-panel__group wf-list-panel__group--query">
              <el-button type="primary" :icon="Search" @click="handleSearch">{{ t('common.query') }}</el-button>
            </div>
            <span class="wf-list-panel__divider" aria-hidden="true" />
            <div class="wf-list-panel__group wf-list-panel__group--action">
              <el-button type="success" :icon="CirclePlusFilled" @click="openCreate">
                {{ t('system.role.add') }}
              </el-button>
              <el-button @click="handleReset">{{ t('common.reset') }}</el-button>
            </div>
          </div>
        </div>

        <WfTable v-loading="loading" class="wf-role-table" :data="tableData" row-key="id">
          <el-table-column :label="t('system.role.roleCode')" min-width="128" show-overflow-tooltip>
            <template #default="{ row }">
              <span class="wf-role-code">{{ row.roleCode }}</span>
            </template>
          </el-table-column>
          <el-table-column prop="roleName" :label="t('system.role.roleName')" min-width="140" show-overflow-tooltip />
          <el-table-column :label="t('common.sort')" width="72" align="center" prop="sort" />
          <el-table-column :label="t('common.status')" width="88" align="center">
            <template #default="{ row }">
              <WfStatusTag :status="row.status" />
            </template>
          </el-table-column>
          <el-table-column :label="t('common.createBy')" min-width="88" show-overflow-tooltip>
            <template #default="{ row }">{{ formatUserName(row.createByName, row.createBy) }}</template>
          </el-table-column>
          <el-table-column :label="t('common.createTime')" width="148" show-overflow-tooltip>
            <template #default="{ row }">
              <span class="wf-role-time">{{ formatDateShort(row.createTime) }}</span>
            </template>
          </el-table-column>
          <el-table-column :label="t('common.updateBy')" min-width="88" show-overflow-tooltip>
            <template #default="{ row }">{{ formatUserName(row.updateByName, row.updateBy) }}</template>
          </el-table-column>
          <el-table-column :label="t('common.updateTime')" width="148" show-overflow-tooltip>
            <template #default="{ row }">
              <span class="wf-role-time">{{ formatDateShort(row.updateTime) }}</span>
            </template>
          </el-table-column>
          <el-table-column
            :label="t('common.actions')"
            width="220"
            align="center"
            fixed="right"
            class-name="wf-role-actions-cell"
          >
            <template #default="{ row }">
              <div class="wf-role-actions">
                <el-button link type="primary" @click="openEdit(row)">{{ t('common.edit') }}</el-button>
                <el-button link type="primary" @click="openAssignPerm(row)">{{ t('system.role.assignMenuPerm') }}</el-button>
                <el-button link type="danger" @click="handleDelete(row)">{{ t('common.delete') }}</el-button>
              </div>
            </template>
          </el-table-column>
          <template #empty>
            <el-empty :description="t('system.role.empty')" :image-size="96" />
          </template>
        </WfTable>
      </div>

      <template #pager>
        <WfPagePager
          v-model:current-page="query.pageIndex"
          v-model:page-size="query.pageSize"
          :total="total"
          @change="loadData"
          @size-change="handleSearch"
        />
      </template>
    </WfPageBody>

    <el-dialog
      v-model="dialogVisible"
      :title="editingId ? t('system.role.edit') : t('system.role.add')"
      width="520px"
      destroy-on-close
      align-center
    >
      <el-form :model="form" label-width="88px">
        <el-form-item :label="t('system.role.roleName')">
          <el-input v-model="form.roleName" />
        </el-form-item>
        <el-form-item :label="t('system.role.roleCode')">
          <el-input v-model="form.roleCode" :disabled="!!editingId" />
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
      <p v-if="!editingId" class="wf-role-form-tip">{{ t('system.role.createPermTip') }}</p>
      <template #footer>
        <WfDialogFooter :loading="submitting" @cancel="dialogVisible = false" @confirm="submitForm" />
      </template>
    </el-dialog>

    <el-dialog
      v-model="permDialogVisible"
      :title="assignPermTitle"
      width="760px"
      class="wf-role-perm-dialog"
      destroy-on-close
      align-center
    >
      <p class="wf-role-perm-dialog__hint">{{ t('system.role.assignMenuPermHint') }}</p>
      <el-tabs v-model="permTab" class="wf-role-perm-dialog__tabs">
        <el-tab-pane :label="t('system.role.webMenuPerm')" name="web" />
        <el-tab-pane :label="t('system.role.mobileMenuPerm')" name="mobile" />
        <el-tab-pane :label="t('system.role.desktopMenuPerm')" name="desktop" />
      </el-tabs>
      <div class="wf-role-perm-dialog__body">
        <WfMenuPermTree
          v-show="permTab === 'web'"
          v-model="webMenuIds"
          :data="webMenuTree"
        />
        <WfMenuPermTree
          v-show="permTab === 'mobile'"
          v-model="mobileMenuIds"
          :data="mobileMenuTree"
        />
        <WfMenuPermTree
          v-show="permTab === 'desktop'"
          v-model="desktopMenuIds"
          :data="desktopMenuTree"
        />
      </div>
      <template #footer>
        <WfDialogFooter :loading="permSubmitting" @cancel="permDialogVisible = false" @confirm="submitPerm" />
      </template>
    </el-dialog>
  </WfPage>
</template>

<style scoped lang="scss">
.wf-role-page {
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
    flex-shrink: 0;
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
    line-height: 32px;
  }

  &__count {
    font-size: 12px;
    font-weight: 400;
    color: var(--wf-text-muted);
  }

  &__title-icon {
    font-size: 18px;
    color: var(--wf-primary);
  }

  &__desc {
    margin: 0;
    font-size: 12px;
    font-weight: 400;
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

  &__select {
    width: 120px;
  }
}

.wf-role-code {
  font-family: ui-monospace, monospace;
  font-size: 12px;
  color: var(--wf-text-secondary);
}

.wf-role-time {
  font-size: 12px;
  color: var(--wf-text-secondary);
  font-variant-numeric: tabular-nums;
}

.wf-role-table {
  flex: 1;
  min-height: 0;
}

.wf-role-actions {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-wrap: wrap;
  gap: 2px 4px;
}

.wf-role-form-tip {
  margin: 0;
  font-size: 12px;
  color: var(--wf-text-muted);
  line-height: 1.5;
}

.wf-role-perm-dialog {
  &__hint {
    margin: 0 0 10px;
    font-size: 12px;
    color: var(--wf-text-muted);
    line-height: 1.5;
  }

  &__tabs {
    :deep(.el-tabs__header) {
      margin-bottom: 10px;
    }
  }

  &__body {
    min-height: 420px;
    display: flex;
    flex-direction: column;
  }
}
</style>
