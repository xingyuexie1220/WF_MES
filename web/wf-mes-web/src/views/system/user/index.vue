<script setup lang="ts">
defineOptions({ name: 'SystemUser' })
import { computed, onMounted, reactive, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { ElMessage, ElMessageBox } from 'element-plus'
import { CirclePlusFilled, Delete, Download, EditPen, Key, Search, User } from '@element-plus/icons-vue'
import { useUserStore } from '@/stores/auth/user'
import { getAllRolesApi } from '@/api/system/role'
import { getDeptTreeApi } from '@/api/system/dept'
import {
  createUserApi,
  deleteUserApi,
  getUserPageApi,
  resetPasswordApi,
  updateUserApi
} from '@/api/system/user'
import { WfDialogFooter, WfPage, WfPageBody, WfPagePager, WfStatusTag, WfTable } from '@/components/page'
import { exportExcel } from '@/utils/exportExcel'
import type { FormInstance, FormRules } from 'element-plus'
import type { DeptItem } from '@/types/system/dept'
import type { RoleItem } from '@/types/system/role'
import type { UserItem } from '@/types/system/user'

const { t } = useI18n()
const userStore = useUserStore()
const loading = ref(false)
const submitting = ref(false)
const deletingId = ref<number | null>(null)
const tableData = ref<UserItem[]>([])
const total = ref(0)
const roles = ref<RoleItem[]>([])
const deptTree = ref<DeptItem[]>([])
const formRef = ref<FormInstance>()

const dialogVisible = ref(false)

const editingId = ref<number | null>(null)

const query = reactive({
  pageIndex: 1,
  pageSize: 20,
  userName: '',
  status: undefined as number | undefined,
  deptId: undefined as number | undefined
})

const form = reactive({
  userName: '',
  password: '',
  nickName: '',
  email: '',
  deptId: 0,
  status: 1,
  remark: '',
  roleIds: [] as number[],
  positionIds: [] as number[]
})

const canAdd = computed(() => userStore.hasPermission('system:user:add'))
const canEdit = computed(() => userStore.hasPermission('system:user:edit'))
const canDelete = computed(() => userStore.hasPermission('system:user:delete'))

const roleNameMap = computed(() => {
  const map = new Map<number, string>()
  for (const role of roles.value) {
    map.set(role.id, role.roleName)
  }
  return map
})

const formRules = computed<FormRules>(() => ({
  userName: [{ required: true, message: t('system.user.validateUserName'), trigger: 'blur' }],
  password: editingId.value
    ? []
    : [{ required: true, message: t('system.user.validatePassword'), trigger: 'blur' }],
  deptId: [{ required: true, message: t('system.user.validateDept'), trigger: 'change' }]
}))

function getRoleNames(roleIds?: number[]) {
  if (!roleIds?.length) {
    return []
  }
  return roleIds.map((id) => roleNameMap.value.get(id)).filter((name): name is string => !!name)
}

function displayOptional(value?: string | null) {
  return value?.trim() || ''
}

function formatUserName(name?: string | null, userId?: number) {
  if (name) {
    return name
  }
  if (userId) {
    return String(userId)
  }
  return t('common.none')
}

function formatDateShort(value?: string | null) {
  if (!value) {
    return t('common.none')
  }
  const date = new Date(value)
  if (Number.isNaN(date.getTime()) || date.getFullYear() < 1970) {
    return t('common.none')
  }
  const y = date.getFullYear()
  const m = String(date.getMonth() + 1).padStart(2, '0')
  const d = String(date.getDate()).padStart(2, '0')
  const hh = String(date.getHours()).padStart(2, '0')
  const mm = String(date.getMinutes()).padStart(2, '0')
  return `${y}-${m}-${d} ${hh}:${mm}`
}

async function loadData() {
  loading.value = true
  try {
    const params: Record<string, unknown> = {
      pageIndex: query.pageIndex,
      pageSize: query.pageSize
    }
    if (query.userName.trim()) {
      params.userName = query.userName.trim()
    }
    if (query.status !== undefined) {
      params.status = query.status
    }
    if (query.deptId) {
      params.deptId = query.deptId
    }

    const data = await getUserPageApi(params)
    tableData.value = data.items
    total.value = data.total
  } finally {
    loading.value = false
  }
}

async function loadOptions() {
  const [roleList, deptList] = await Promise.all([getAllRolesApi(), getDeptTreeApi()])
  roles.value = roleList
  deptTree.value = deptList
}

function handleSearch() {
  query.pageIndex = 1
  loadData()
}

function handleReset() {
  query.userName = ''
  query.status = undefined
  query.deptId = undefined
  query.pageIndex = 1
  loadData()
}

function resetForm() {
  Object.assign(form, {
    userName: '',
    password: '',
    nickName: '',
    email: '',
    deptId: deptTree.value[0]?.id || 0,
    status: 1,
    remark: '',
    roleIds: [],
    positionIds: []
  })
}

function openCreate() {
  editingId.value = null
  resetForm()
  dialogVisible.value = true
}

function openEdit(row: UserItem) {
  editingId.value = row.id
  Object.assign(form, {
    userName: row.userName,
    password: '',
    nickName: row.nickName ?? '',
    email: row.email ?? '',
    deptId: row.deptId,
    status: row.status,
    remark: row.remark ?? '',
    roleIds: [...row.roleIds],
    positionIds: [...row.positionIds]
  })
  dialogVisible.value = true
}

async function submitForm() {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) {
    return
  }

  submitting.value = true
  try {
    if (editingId.value) {
      await updateUserApi(editingId.value, form)
      ElMessage.success(t('common.updateSuccess'))
    } else {
      await createUserApi(form)
      ElMessage.success(t('common.createSuccess'))
    }
    dialogVisible.value = false
    await loadData()
  } finally {
    submitting.value = false
  }
}

async function handleDelete(row: UserItem) {
  try {
    await ElMessageBox.confirm(t('system.user.confirmDelete', { name: row.userName }), t('common.tip'), {
      type: 'warning',
      confirmButtonText: t('common.confirm'),
      cancelButtonText: t('common.cancel')
    })
    deletingId.value = row.id
    try {
      await deleteUserApi(row.id)
      ElMessage.success(t('common.deleteSuccess'))
      await loadData()
    } finally {
      deletingId.value = null
    }
  } catch {
    /* 用户取消 */
  }
}

async function handleResetPassword(row: UserItem) {
  try {
    const { value } = await ElMessageBox.prompt(t('system.user.newPassword'), t('system.user.resetPassword'), {
      inputType: 'password',
      confirmButtonText: t('common.confirm'),
      cancelButtonText: t('common.cancel')
    })
    if (value) {
      await resetPasswordApi(row.id, value)
      ElMessage.success(t('system.user.passwordResetSuccess'))
    }
  } catch {
    /* 用户取消 */
  }
}

function handleExport() {
  if (tableData.value.length === 0) {
    ElMessage.warning(t('system.user.empty'))
    return
  }
  const timestamp = new Date().toISOString().slice(0, 10).replace(/-/g, '')
  exportExcel(
    `${t('system.user.exportFileName')}_${timestamp}.xlsx`,
    t('system.user.title'),
    [
      { header: t('system.user.userName'), key: 'userName', width: 14 },
      { header: t('system.user.nickName'), key: 'nickName', width: 14 },
      { header: t('system.user.dept'), key: 'deptName', width: 18 },
      {
        header: t('system.user.roles'),
        width: 16,
        formatter: (row) => getRoleNames(row.roleIds).join('、') || t('system.user.roleEmpty')
      },
      { header: t('system.user.email'), key: 'email', width: 22 },
      {
        header: t('common.status'),
        width: 10,
        formatter: (row) => (row.status === 1 ? t('common.enabled') : t('common.disabled'))
      },
      {
        header: t('common.createBy'),
        width: 12,
        formatter: (row) => formatUserName(row.createByName, row.createBy)
      },
      {
        header: t('common.createTime'),
        width: 18,
        formatter: (row) => formatDateShort(row.createTime)
      },
      {
        header: t('common.updateBy'),
        width: 12,
        formatter: (row) => formatUserName(row.updateByName, row.updateBy)
      },
      {
        header: t('common.updateTime'),
        width: 18,
        formatter: (row) => formatDateShort(row.updateTime)
      }
    ],
    tableData.value
  )
  ElMessage.success(t('system.user.exportSuccess'))
}

onMounted(async () => {
  await loadOptions()
  await loadData()
})
</script>

<template>
  <WfPage class="wf-user-page">
    <WfPageBody class="wf-user-page__body">
      <div class="wf-list-panel">
        <div class="wf-list-panel__head">
          <div class="wf-list-panel__title">
            <el-icon class="wf-list-panel__title-icon"><User /></el-icon>
            <span>{{ t('system.user.title') }}</span>
            <span class="wf-list-panel__desc">{{ t('system.user.roleHint') }}</span>
          </div>
          <div class="wf-list-panel__toolbar">
            <div class="wf-list-panel__group wf-list-panel__group--filter">
              <el-input
                v-model="query.userName"
                clearable
                :prefix-icon="Search"
                :placeholder="t('system.user.searchUserName')"
                class="wf-list-panel__input"
                @keyup.enter="handleSearch"
              />
              <el-tree-select
                v-model="query.deptId"
                :data="deptTree"
                node-key="id"
                clearable
                :placeholder="t('system.user.deptPlaceholder')"
                :props="{ label: 'deptName', value: 'id', children: 'children' }"
                check-strictly
                class="wf-list-panel__select wf-list-panel__select--dept"
              />
              <el-select
                v-model="query.status"
                clearable
                :placeholder="t('system.user.searchStatus')"
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
              <el-button v-if="canAdd" type="success" :icon="CirclePlusFilled" @click="openCreate">
                {{ t('system.user.add') }}
              </el-button>
              <el-button @click="handleReset">{{ t('common.reset') }}</el-button>
              <el-button plain :icon="Download" @click="handleExport">{{ t('common.export') }}</el-button>
            </div>
          </div>
        </div>

        <WfTable v-loading="loading" class="wf-user-table" :data="tableData" row-key="id">
          <el-table-column :label="t('system.user.userName')" min-width="100" show-overflow-tooltip prop="userName" />
          <el-table-column :label="t('system.user.nickName')" min-width="100" show-overflow-tooltip>
            <template #default="{ row }">
              {{ displayOptional(row.nickName) }}
            </template>
          </el-table-column>
          <el-table-column :label="t('system.user.dept')" min-width="120" show-overflow-tooltip>
            <template #default="{ row }">
              {{ row.deptName || t('common.none') }}
            </template>
          </el-table-column>
          <el-table-column :label="t('system.user.roles')" min-width="110" show-overflow-tooltip>
            <template #default="{ row }">
              <div class="wf-user-roles">
                <template v-if="getRoleNames(row.roleIds).length">
                  <el-tag
                    v-for="name in getRoleNames(row.roleIds).slice(0, 2)"
                    :key="name"
                    size="small"
                    type="info"
                    disable-transitions
                    class="wf-user-roles__tag"
                  >
                    {{ name }}
                  </el-tag>
                  <span v-if="getRoleNames(row.roleIds).length > 2" class="wf-user-roles__more">
                    +{{ getRoleNames(row.roleIds).length - 2 }}
                  </span>
                </template>
                <span v-else class="wf-user-roles__empty">{{ t('system.user.roleEmpty') }}</span>
              </div>
            </template>
          </el-table-column>
          <el-table-column :label="t('system.user.email')" min-width="120" show-overflow-tooltip>
            <template #default="{ row }">
              {{ displayOptional(row.email) }}
            </template>
          </el-table-column>
          <el-table-column :label="t('common.status')" width="80" align="center">
            <template #default="{ row }">
              <WfStatusTag :status="row.status" />
            </template>
          </el-table-column>
          <el-table-column :label="t('common.createBy')" min-width="96" show-overflow-tooltip>
            <template #default="{ row }">
              {{ formatUserName(row.createByName, row.createBy) }}
            </template>
          </el-table-column>
          <el-table-column :label="t('common.createTime')" width="152" show-overflow-tooltip>
            <template #default="{ row }">
              <span class="wf-user-time">{{ formatDateShort(row.createTime) }}</span>
            </template>
          </el-table-column>
          <el-table-column :label="t('common.updateBy')" min-width="96" show-overflow-tooltip>
            <template #default="{ row }">
              {{ formatUserName(row.updateByName, row.updateBy) }}
            </template>
          </el-table-column>
          <el-table-column :label="t('common.updateTime')" width="152" show-overflow-tooltip>
            <template #default="{ row }">
              <span class="wf-user-time">{{ formatDateShort(row.updateTime) }}</span>
            </template>
          </el-table-column>
          <el-table-column
            :label="t('common.actions')"
            width="124"
            align="center"
            class-name="wf-user-actions-cell"
          >
            <template #default="{ row }">
              <div class="wf-user-actions">
                <el-tooltip v-if="canEdit" :content="t('common.edit')" placement="top">
                  <el-button class="wf-user-actions__btn is-edit" text @click="openEdit(row)">
                    <el-icon><EditPen /></el-icon>
                  </el-button>
                </el-tooltip>
                <el-tooltip v-if="canEdit" :content="t('system.user.resetPassword')" placement="top">
                  <el-button class="wf-user-actions__btn is-key" text @click="handleResetPassword(row)">
                    <el-icon><Key /></el-icon>
                  </el-button>
                </el-tooltip>
                <el-tooltip v-if="canDelete" :content="t('common.delete')" placement="top">
                  <el-button
                    class="wf-user-actions__btn is-delete"
                    text
                    :loading="deletingId === row.id"
                    @click="handleDelete(row)"
                  >
                    <el-icon><Delete /></el-icon>
                  </el-button>
                </el-tooltip>
              </div>
            </template>
          </el-table-column>
          <template #empty>
            <el-empty :description="t('system.user.empty')" :image-size="96" />
          </template>
        </WfTable>
      </div>

      <template #pager>
        <WfPagePager
          v-model:current-page="query.pageIndex"
          v-model:page-size="query.pageSize"
          :total="total"
          :page-sizes="[10, 20, 50, 100]"
          layout="total, sizes, prev, pager, next, jumper"
          @change="loadData"
          @size-change="handleSearch"
        />
      </template>
    </WfPageBody>

    <el-dialog
      v-model="dialogVisible"
      :title="editingId ? t('system.user.edit') : t('system.user.add')"
      width="720px"
      destroy-on-close
    >
      <el-form ref="formRef" :model="form" :rules="formRules" label-width="88px">
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item :label="t('system.user.userName')" prop="userName">
              <el-input v-model="form.userName" :disabled="!!editingId" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item v-if="!editingId" :label="t('system.user.password')" prop="password">
              <el-input v-model="form.password" type="password" show-password />
            </el-form-item>
            <el-form-item v-else :label="t('system.user.nickName')">
              <el-input v-model="form.nickName" />
            </el-form-item>
          </el-col>
          <el-col v-if="!editingId" :span="12">
            <el-form-item :label="t('system.user.nickName')">
              <el-input v-model="form.nickName" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item :label="t('system.user.email')">
              <el-input v-model="form.email" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item :label="t('system.user.dept')" prop="deptId">
              <el-tree-select
                v-model="form.deptId"
                :data="deptTree"
                node-key="id"
                :props="{ label: 'deptName', value: 'id', children: 'children' }"
                check-strictly
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item :label="t('system.user.roles')">
              <el-select v-model="form.roleIds" multiple collapse-tags collapse-tags-tooltip style="width: 100%">
                <el-option v-for="item in roles" :key="item.id" :label="item.roleName" :value="item.id" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item :label="t('common.status')">
              <el-radio-group v-model="form.status">
                <el-radio :value="1">{{ t('common.enabled') }}</el-radio>
                <el-radio :value="0">{{ t('common.disabled') }}</el-radio>
              </el-radio-group>
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item :label="t('common.remark')">
              <el-input v-model="form.remark" type="textarea" :rows="3" />
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
      <template #footer>
        <WfDialogFooter :loading="submitting" @cancel="dialogVisible = false" @confirm="submitForm" />
      </template>
    </el-dialog>
  </WfPage>
</template>

<style scoped lang="scss">
.wf-user-page {
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

  &__title {
    display: inline-flex;
    align-items: center;
    gap: 8px;
    font-size: 15px;
    font-weight: 600;
    color: var(--wf-text);
    line-height: 32px;
  }

  &__title-icon {
    font-size: 18px;
    color: var(--wf-primary);
  }

  &__desc {
    font-size: 12px;
    font-weight: 400;
    color: var(--wf-text-muted);
  }

  &__toolbar {
    display: flex;
    align-items: center;
    justify-content: flex-end;
    flex-wrap: wrap;
    gap: 10px;
    flex: 1;
    min-width: 320px;
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
  }

  &__select {
    width: 120px;

    &--dept {
      width: 156px;
    }
  }
}

.wf-user-roles {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 4px;

  &__tag {
    max-width: 100%;
  }

  &__more {
    font-size: 12px;
    color: var(--wf-text-muted);
  }

  &__empty {
    font-size: 12px;
    color: #c0c4cc;
  }
}

.wf-user-time {
  font-size: 12px;
  color: var(--wf-text-secondary);
  font-variant-numeric: tabular-nums;
}

.wf-user-table {
  :deep(.wf-user-actions-cell.el-table__cell) {
    overflow: visible;
    text-overflow: clip;
  }

  :deep(.wf-user-actions-cell .cell) {
    overflow: visible !important;
    text-overflow: clip !important;
    white-space: nowrap;
    padding-left: 4px;
    padding-right: 16px;
  }
}

.wf-user-actions {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 2px;
  flex-wrap: nowrap;

  &__btn {
    flex-shrink: 0;
    width: 28px;
    height: 28px;
    padding: 0;
    border-radius: 4px;
    font-size: 16px;

    &.is-edit {
      color: #e6a23c;
    }

    &.is-key {
      color: #909399;
    }

    &.is-delete {
      color: #f56c6c;
    }

    &:hover {
      background: #f5f7fa;
    }
  }
}
</style>
