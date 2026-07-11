<script setup lang="ts">
defineOptions({ name: 'SystemDept' })
import { computed, nextTick, onMounted, reactive, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { ElMessage, ElMessageBox } from 'element-plus'
import { CirclePlusFilled, Download, Search } from '@element-plus/icons-vue'
import { useUserStore } from '@/stores/auth/user'
import { createDeptApi, deleteDeptApi, getDeptByIdApi, getDeptTreeApi, updateDeptApi } from '@/api/system/dept'
import { WfDialogFooter, WfStatusTag, WfTypeTag } from '@/components/page'
import { filterAdminOrgTree } from '@/utils/orgTree'
import { exportExcel } from '@/utils/exportExcel'
import type { ElTree, FormInstance, FormRules } from 'element-plus'
import type { DeptItem } from '@/types/system/dept'

/** 工厂内组织：车间(1) → 产线(2) → 班组(3) */
const ORG_WORKSHOP = 1
const ORG_LINE = 2
const ORG_TEAM = 3

interface DeptExportRow {
  deptName: string
  deptCode: string
  deptType: string
  parentPath: string
  level: number
  status: string
  sort: number
  remark: string
}

const { t } = useI18n()
const userStore = useUserStore()
const treeRef = ref<InstanceType<typeof ElTree>>()
const addFormRef = ref<FormInstance>()
const editFormRef = ref<FormInstance>()
const treeLoading = ref(false)
const treeData = ref<DeptItem[]>([])
const treeFilter = ref('')
const deletingId = ref<number | null>(null)

const dialogVisible = reactive({
  add: false,
  edit: false,
  detail: false
})

const addSubmitting = ref(false)
const editSubmitting = ref(false)
const currentEditDept = ref<DeptItem | null>(null)
const currentDetailDept = ref<DeptItem | null>(null)

const addForm = reactive({
  parentId: undefined as number | undefined,
  deptCode: '',
  deptName: '',
  deptType: ORG_WORKSHOP,
  sort: 1,
  remark: ''
})

const editForm = reactive({
  parentId: 0,
  deptCode: '',
  deptName: '',
  deptType: ORG_LINE,
  status: 1,
  remark: ''
})

const canAdd = computed(() => userStore.hasPermission('system:dept:add'))
const canEdit = computed(() => userStore.hasPermission('system:dept:edit'))
const canDelete = computed(() => userStore.hasPermission('system:dept:delete'))
const canView = computed(() => userStore.hasPermission('system:dept:list'))

const deptTypeMap = computed(() => ({
  [ORG_WORKSHOP]: t('system.dept.typeWorkshop'),
  [ORG_LINE]: t('system.dept.typeLine'),
  [ORG_TEAM]: t('system.dept.typeTeam')
}))

const parentTreeOptions = computed(() => filterParentNodes(treeData.value))

const orgCount = computed(() => countNodes(treeData.value))

const orgStats = computed(() => {
  const stats = { workshop: 0, line: 0, team: 0 }
  const walk = (nodes: DeptItem[]) => {
    for (const node of nodes) {
      if (node.deptType === ORG_WORKSHOP) stats.workshop++
      else if (node.deptType === ORG_LINE) stats.line++
      else if (node.deptType === ORG_TEAM) stats.team++
      if (node.children?.length) walk(node.children)
    }
  }
  walk(treeData.value)
  return stats
})

const statSummary = computed(() =>
  t('system.dept.statSummary', {
    workshop: orgStats.value.workshop,
    line: orgStats.value.line,
    team: orgStats.value.team
  })
)

const formRules = computed<FormRules>(() => ({
  deptCode: [{ required: true, message: t('system.dept.validateCode'), trigger: 'blur' }],
  deptName: [{ required: true, message: t('system.dept.validateName'), trigger: 'blur' }]
}))

const addFormTypeLabel = computed(
  () => deptTypeMap.value[addForm.deptType as keyof typeof deptTypeMap.value] || ''
)

const editFormTypeLabel = computed(
  () => deptTypeMap.value[editForm.deptType as keyof typeof deptTypeMap.value] || ''
)

watch(treeFilter, (val) => {
  treeRef.value?.filter(val)
  if (!val.trim()) {
    nextTick(() => expandTreeLevels(2))
  }
})

function filterTreeData(nodes: DeptItem[], nameKey: string, codeKey: string): DeptItem[] {
  const result: DeptItem[] = []
  for (const node of nodes) {
    const filteredChildren = node.children?.length ? filterTreeData(node.children, nameKey, codeKey) : []
    const nameMatch = !nameKey || node.deptName.toLowerCase().includes(nameKey)
    const codeMatch = !codeKey || node.deptCode.toLowerCase().includes(codeKey)
    if (nameMatch && codeMatch) {
      result.push({ ...node, children: node.children })
    } else if (filteredChildren.length) {
      result.push({ ...node, children: filteredChildren })
    }
  }
  return result
}

const exportTreeData = computed(() => {
  const nameKey = treeFilter.value.trim().toLowerCase()
  const codeKey = treeFilter.value.trim().toLowerCase()
  if (!nameKey && !codeKey) {
    return treeData.value
  }
  return filterTreeData(treeData.value, nameKey, codeKey)
})

function flattenForExport(nodes: DeptItem[], parentPath = '', level = 1): DeptExportRow[] {
  const rows: DeptExportRow[] = []
  for (const node of nodes) {
    const currentPath = parentPath ? `${parentPath} / ${node.deptName}` : node.deptName
    rows.push({
      deptName: node.deptName,
      deptCode: node.deptCode,
      deptType: deptTypeMap.value[node.deptType as keyof typeof deptTypeMap.value] || '',
      parentPath: parentPath || t('system.dept.rootLevel'),
      level,
      status: node.status === 1 ? t('common.enabled') : t('common.disabled'),
      sort: node.sort,
      remark: node.remark ?? ''
    })
    if (node.children?.length) {
      rows.push(...flattenForExport(node.children, currentPath, level + 1))
    }
  }
  return rows
}

function handleExportExcel() {
  const rows = flattenForExport(exportTreeData.value)
  if (rows.length === 0) {
    ElMessage.warning(t('system.dept.empty'))
    return
  }

  const timestamp = new Date().toISOString().slice(0, 10).replace(/-/g, '')
  exportExcel<DeptExportRow>(
    `${t('system.dept.exportFileName')}_${timestamp}.xlsx`,
    t('system.dept.exportFileName'),
    [
      { header: t('system.dept.colLevel'), key: 'level', width: 8 },
      { header: t('system.dept.deptName'), key: 'deptName', width: 28 },
      { header: t('system.dept.deptCode'), key: 'deptCode', width: 18 },
      { header: t('system.dept.deptType'), key: 'deptType', width: 12 },
      { header: t('system.dept.colParentPath'), key: 'parentPath', width: 36 },
      { header: t('common.status'), key: 'status', width: 10 },
      { header: t('common.sort'), key: 'sort', width: 8 },
      { header: t('common.remark'), key: 'remark', width: 24 }
    ],
    rows
  )
  ElMessage.success(t('system.dept.exportSuccess'))
}

function countNodes(nodes: DeptItem[]): number {
  return nodes.reduce((sum, node) => sum + 1 + (node.children?.length ? countNodes(node.children) : 0), 0)
}

function filterParentNodes(nodes: DeptItem[]): DeptItem[] {
  const result: DeptItem[] = []
  for (const node of nodes) {
    if (resolveChildType(node.deptType) === null) {
      continue
    }
    const children = node.children?.length ? filterParentNodes(node.children) : []
    result.push({ ...node, children: children.length ? children : undefined })
  }
  return result
}

function resolveChildType(parentType: number): number | null {
  if (parentType === ORG_WORKSHOP) return ORG_LINE
  if (parentType === ORG_LINE) return ORG_TEAM
  return null
}

function canAddChild(row: DeptItem) {
  return canAdd.value && resolveChildType(row.deptType) !== null
}

function findNodeById(nodes: DeptItem[], id: number): DeptItem | null {
  for (const node of nodes) {
    if (node.id === id) {
      return node
    }
    if (node.children?.length) {
      const found = findNodeById(node.children, id)
      if (found) {
        return found
      }
    }
  }
  return null
}

function filterTreeNode(value: string, data: DeptItem) {
  if (!value) {
    return true
  }
  const q = value.trim().toLowerCase()
  return data.deptName.toLowerCase().includes(q) || data.deptCode.toLowerCase().includes(q)
}

function getSiblingNodes(parentId?: number): DeptItem[] {
  if (parentId === undefined || parentId === 0) {
    return treeData.value
  }
  const parent = findNodeById(treeData.value, parentId)
  return parent?.children ?? []
}

function resolveNextSort(parentId?: number): number {
  const siblings = getSiblingNodes(parentId)
  if (siblings.length === 0) {
    return 1
  }
  return Math.max(...siblings.map((item) => item.sort)) + 1
}

function resolveAddDeptType(parentId?: number) {
  if (parentId === undefined || parentId === 0) {
    addForm.deptType = ORG_WORKSHOP
    return
  }
  const parent = findNodeById(treeData.value, parentId)
  const childType = parent ? resolveChildType(parent.deptType) : null
  if (childType === null) {
    addForm.parentId = undefined
    addForm.deptType = ORG_WORKSHOP
    ElMessage.warning(t('system.dept.leafNoChild'))
    return
  }
  addForm.deptType = childType
}

function setTreeExpand(expand: boolean) {
  if (!treeRef.value) {
    return
  }
  for (const node of Object.values(treeRef.value.store.nodesMap)) {
    node.expanded = expand
  }
}

function expandTreeLevels(maxLevel: number) {
  if (!treeRef.value) {
    return
  }
  for (const node of Object.values(treeRef.value.store.nodesMap)) {
    node.expanded = node.level < maxLevel
  }
}

async function loadTree() {
  treeLoading.value = true
  try {
    treeData.value = filterAdminOrgTree(await getDeptTreeApi())
    await nextTick()
    if (treeFilter.value.trim()) {
      treeRef.value?.filter(treeFilter.value)
    } else {
      expandTreeLevels(2)
    }
  } finally {
    treeLoading.value = false
  }
}

function resetAddForm(parent?: DeptItem) {
  Object.assign(addForm, {
    parentId: parent?.id,
    deptCode: '',
    deptName: '',
    deptType: parent ? resolveChildType(parent.deptType) ?? ORG_WORKSHOP : ORG_WORKSHOP,
    sort: 1,
    remark: ''
  })
  addForm.sort = resolveNextSort(addForm.parentId)
}

function openAddDialog(parent?: DeptItem) {
  resetAddForm(parent)
  if (!parent) {
    resolveAddDeptType(addForm.parentId)
  }
  addForm.sort = resolveNextSort(addForm.parentId)
  dialogVisible.add = true
}

function onAddParentChange(parentId?: number) {
  resolveAddDeptType(parentId)
  addForm.sort = resolveNextSort(parentId)
}

async function handleAddSubmit() {
  if (!addFormRef.value) {
    return
  }
  const valid = await addFormRef.value.validate().catch(() => false)
  if (!valid) {
    return
  }

  addSubmitting.value = true
  try {
    await createDeptApi({
      parentId: addForm.parentId ?? 0,
      deptCode: addForm.deptCode.trim(),
      deptName: addForm.deptName.trim(),
      deptType: addForm.deptType,
      sort: addForm.sort,
      status: 1,
      remark: addForm.remark.trim()
    })
    ElMessage.success(t('common.createSuccess'))
    dialogVisible.add = false
    await loadTree()
  } finally {
    addSubmitting.value = false
  }
}

function handleEdit(row: DeptItem) {
  currentEditDept.value = row
  Object.assign(editForm, {
    parentId: row.parentId,
    deptCode: row.deptCode,
    deptName: row.deptName,
    deptType: row.deptType,
    status: row.status,
    remark: row.remark ?? ''
  })
  dialogVisible.edit = true
}

async function handleEditSubmit() {
  if (!currentEditDept.value || !editFormRef.value) {
    return
  }
  const valid = await editFormRef.value.validate().catch(() => false)
  if (!valid) {
    return
  }

  editSubmitting.value = true
  try {
    await updateDeptApi(currentEditDept.value.id, {
      parentId: editForm.parentId,
      deptCode: editForm.deptCode.trim(),
      deptName: editForm.deptName.trim(),
      deptType: editForm.deptType,
      sort: currentEditDept.value.sort,
      status: editForm.status,
      remark: editForm.remark.trim()
    })
    ElMessage.success(t('common.updateSuccess'))
    dialogVisible.edit = false
    await loadTree()
  } finally {
    editSubmitting.value = false
  }
}

async function handleDetail(row: DeptItem) {
  currentDetailDept.value = row
  dialogVisible.detail = true
  try {
    const detail = await getDeptByIdApi(row.id)
    currentDetailDept.value = { ...row, ...detail }
  } catch {
    /* 保留列表行数据 */
  }
}

function formatDateTime(value?: string | null) {
  if (!value) {
    return t('common.none')
  }
  const date = new Date(value)
  if (Number.isNaN(date.getTime()) || date.getFullYear() < 1970) {
    return t('common.none')
  }
  return date.toLocaleString()
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

async function handleDelete(row: DeptItem) {
  try {
    await ElMessageBox.confirm(t('system.dept.confirmDelete', { name: row.deptName }), t('common.tip'), {
      type: 'warning',
      confirmButtonText: t('common.confirm'),
      cancelButtonText: t('common.cancel')
    })
    deletingId.value = row.id
    try {
      await deleteDeptApi(row.id)
      ElMessage.success(t('common.deleteSuccess'))
      await loadTree()
    } finally {
      deletingId.value = null
    }
  } catch {
    /* 用户取消 */
  }
}

onMounted(loadTree)
</script>

<template>
  <div class="wf-dept-page">
    <div class="wf-dept-page__header">
      <div class="wf-dept-page__header-main">
        <span v-if="orgCount > 0" class="wf-dept-page__count">{{ orgCount }}</span>
        <p class="wf-dept-page__desc">{{ t('system.dept.orgRule') }}</p>
        <span v-if="orgCount > 0" class="wf-dept-page__stats">{{ statSummary }}</span>
      </div>
      <el-button v-if="canAdd" type="success" :icon="CirclePlusFilled" @click="openAddDialog()">
        {{ t('system.dept.add') }}
      </el-button>
    </div>

    <div class="wf-dept-page__card" v-loading="treeLoading">
      <div v-if="treeData.length > 0" class="wf-dept-page__toolbar">
        <el-input
          v-model="treeFilter"
          :placeholder="t('system.dept.treeFilter')"
          clearable
          :prefix-icon="Search"
          class="wf-dept-page__search"
        />
        <div class="wf-dept-page__toolbar-actions">
          <el-button v-if="canView" link type="primary" :icon="Download" @click="handleExportExcel">
            {{ t('common.export') }}
          </el-button>
          <el-button link type="primary" @click="expandTreeLevels(2)">{{ t('system.dept.expandToCompany') }}</el-button>
          <el-button link type="primary" @click="setTreeExpand(true)">{{ t('system.dept.expandAll') }}</el-button>
          <el-button link type="primary" @click="setTreeExpand(false)">{{ t('system.dept.collapseAll') }}</el-button>
        </div>
      </div>

      <el-empty v-if="!treeLoading && treeData.length === 0" :description="t('system.dept.empty')">
        <el-button v-if="canAdd" type="primary" @click="openAddDialog()">{{ t('system.dept.add') }}</el-button>
      </el-empty>

      <template v-else-if="treeData.length > 0">
        <div class="wf-dept-tree__head">
          <span class="wf-dept-tree__head-name">{{ t('system.dept.colName') }}</span>
          <span class="wf-dept-tree__head-code">{{ t('system.dept.colCode') }}</span>
          <span class="wf-dept-tree__head-type">{{ t('system.dept.colType') }}</span>
          <span class="wf-dept-tree__head-actions">{{ t('system.dept.colActions') }}</span>
        </div>
        <el-scrollbar class="wf-dept-page__tree-scroll">
          <el-tree
            ref="treeRef"
            :data="treeData"
            node-key="id"
            :props="{ children: 'children', label: 'deptName' }"
            :filter-node-method="filterTreeNode"
            :indent="18"
            highlight-current
            class="wf-dept-tree"
          >
            <template #default="{ data }">
              <div class="wf-dept-tree__row" :class="{ 'is-disabled': data.status !== 1 }">
                <span class="wf-dept-tree__name">{{ data.deptName }}</span>
                <span v-if="data.deptCode" class="wf-dept-tree__code">{{ data.deptCode }}</span>
                <span v-else class="wf-dept-tree__code wf-dept-tree__code--empty">—</span>
                <span class="wf-dept-tree__type">
                  <WfTypeTag
                    :type="data.deptType"
                    :label="deptTypeMap[data.deptType as keyof typeof deptTypeMap]"
                  />
                </span>
                <div class="wf-dept-tree__actions">
                  <WfStatusTag v-if="data.status !== 1" :status="data.status" />
                  <el-button v-if="canAddChild(data)" link type="primary" @click.stop="openAddDialog(data)">
                    {{ t('system.dept.addChild') }}
                  </el-button>
                  <el-button v-if="canView" link type="primary" @click.stop="handleDetail(data)">
                    {{ t('system.dept.view') }}
                  </el-button>
                  <el-button v-if="canEdit" link type="primary" @click.stop="handleEdit(data)">
                    {{ t('common.edit') }}
                  </el-button>
                  <el-button
                    v-if="canDelete"
                    link
                    type="danger"
                    :loading="deletingId === data.id"
                    @click.stop="handleDelete(data)"
                  >
                    {{ t('common.delete') }}
                  </el-button>
                </div>
              </div>
            </template>
          </el-tree>
        </el-scrollbar>
      </template>
    </div>

    <el-dialog v-model="dialogVisible.add" :title="t('system.dept.add')" width="640px" destroy-on-close>
      <el-form ref="addFormRef" :model="addForm" :rules="formRules" label-width="100px">
        <el-form-item v-if="treeData.length > 0" :label="t('system.dept.parentOrg')" prop="parentId">
          <el-tree-select
            v-model="addForm.parentId"
            :data="parentTreeOptions"
            node-key="id"
            clearable
            :placeholder="t('system.dept.rootLevel')"
            :props="{ label: 'deptName', value: 'id', children: 'children' }"
            check-strictly
            style="width: 100%"
            @change="onAddParentChange"
          />
        </el-form-item>
        <el-form-item :label="t('system.dept.deptType')">
          <WfTypeTag :type="addForm.deptType" :label="addFormTypeLabel" />
        </el-form-item>
        <el-form-item :label="t('system.dept.deptCode')" prop="deptCode">
          <el-input v-model="addForm.deptCode" />
        </el-form-item>
        <el-form-item :label="t('system.dept.deptName')" prop="deptName">
          <el-input v-model="addForm.deptName" />
        </el-form-item>
        <el-form-item :label="t('common.remark')">
          <el-input v-model="addForm.remark" type="textarea" :rows="3" />
        </el-form-item>
      </el-form>
      <template #footer>
        <WfDialogFooter :loading="addSubmitting" @cancel="dialogVisible.add = false" @confirm="handleAddSubmit" />
      </template>
    </el-dialog>

    <el-dialog v-model="dialogVisible.edit" :title="t('system.dept.edit')" width="640px" destroy-on-close>
      <el-form ref="editFormRef" :model="editForm" :rules="formRules" label-width="100px">
        <el-form-item :label="t('system.dept.deptType')">
          <WfTypeTag :type="editForm.deptType" :label="editFormTypeLabel" />
        </el-form-item>
        <el-form-item :label="t('system.dept.deptCode')" prop="deptCode">
          <el-input v-model="editForm.deptCode" />
        </el-form-item>
        <el-form-item :label="t('system.dept.deptName')" prop="deptName">
          <el-input v-model="editForm.deptName" />
        </el-form-item>
        <el-form-item :label="t('common.status')">
          <el-radio-group v-model="editForm.status">
            <el-radio :value="1">{{ t('common.enabled') }}</el-radio>
            <el-radio :value="0">{{ t('common.disabled') }}</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item :label="t('common.remark')">
          <el-input v-model="editForm.remark" type="textarea" :rows="3" />
        </el-form-item>
      </el-form>
      <template #footer>
        <WfDialogFooter :loading="editSubmitting" @cancel="dialogVisible.edit = false" @confirm="handleEditSubmit" />
      </template>
    </el-dialog>

    <el-dialog v-model="dialogVisible.detail" :title="t('system.dept.view')" width="640px" destroy-on-close>
      <el-descriptions v-if="currentDetailDept" :column="1" border>
        <el-descriptions-item :label="t('system.dept.deptName')">{{ currentDetailDept.deptName }}</el-descriptions-item>
        <el-descriptions-item :label="t('system.dept.deptCode')">{{ currentDetailDept.deptCode }}</el-descriptions-item>
        <el-descriptions-item :label="t('system.dept.deptType')">
          <WfTypeTag
            :type="currentDetailDept.deptType"
            :label="deptTypeMap[currentDetailDept.deptType as keyof typeof deptTypeMap]"
          />
        </el-descriptions-item>
        <el-descriptions-item :label="t('common.status')">
          <WfStatusTag :status="currentDetailDept.status" />
        </el-descriptions-item>
        <el-descriptions-item :label="t('common.remark')">{{ currentDetailDept.remark || t('common.none') }}</el-descriptions-item>
        <el-descriptions-item :label="t('common.createBy')">
          {{ formatUserName(currentDetailDept.createByName, currentDetailDept.createBy) }}
        </el-descriptions-item>
        <el-descriptions-item :label="t('common.createTime')">
          {{ formatDateTime(currentDetailDept.createTime) }}
        </el-descriptions-item>
        <el-descriptions-item :label="t('common.updateBy')">
          {{ formatUserName(currentDetailDept.updateByName, currentDetailDept.updateBy) }}
        </el-descriptions-item>
        <el-descriptions-item :label="t('common.updateTime')">
          {{ formatDateTime(currentDetailDept.updateTime) }}
        </el-descriptions-item>
      </el-descriptions>
      <template #footer>
        <el-button @click="dialogVisible.detail = false">{{ t('common.cancel') }}</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped lang="scss">
.wf-dept-page {
  padding: 12px;
  background: var(--wf-content-bg);
  height: calc(100vh - var(--wf-header-height) - var(--wf-tabs-height));
  display: flex;
  flex-direction: column;
  box-sizing: border-box;

  &__header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 16px;
    padding: 10px 16px;
    background: #fff;
    border: 1px solid var(--wf-border);
    border-radius: 8px;
    box-shadow: var(--wf-card-shadow);
    margin-bottom: 12px;
    flex-shrink: 0;
  }

  &__header-main {
    display: flex;
    align-items: center;
    gap: 10px;
    flex-wrap: wrap;
    min-width: 0;
  }

  &__count {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    min-width: 28px;
    height: 24px;
    padding: 0 8px;
    font-size: 13px;
    font-weight: 600;
    color: var(--wf-tab-primary);
    background: var(--wf-tab-active-bg);
    border-radius: 12px;
  }

  &__desc {
    margin: 0;
    font-size: 13px;
    color: var(--wf-text-secondary);
  }

  &__stats {
    font-size: 12px;
    color: var(--wf-text-muted);
    padding-left: 10px;
    border-left: 1px solid #eef0f3;
  }

  &__card {
    flex: 1;
    min-height: 0;
    display: flex;
    flex-direction: column;
    padding: 12px;
    background: #fff;
    border: 1px solid var(--wf-border);
    border-radius: 8px;
    box-shadow: var(--wf-card-shadow);
  }

  &__toolbar {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    margin-bottom: 10px;
    padding-bottom: 10px;
    border-bottom: 1px solid #eef0f3;
    flex-shrink: 0;
  }

  &__search {
    width: 260px;
  }

  &__toolbar-actions {
    display: flex;
    gap: 4px;
    flex-shrink: 0;
  }

  &__tree-scroll {
    flex: 1;
    min-height: 0;
  }
}

.wf-dept-tree__head {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 12px 8px 28px;
  margin-bottom: 4px;
  font-size: 12px;
  font-weight: 600;
  color: var(--wf-text-secondary);
  background: #f7f8fa;
  border-radius: 6px;
  flex-shrink: 0;

  &-name {
    flex: 1;
    min-width: 0;
  }

  &-code {
    width: 120px;
    flex-shrink: 0;
  }

  &-type {
    width: 72px;
    flex-shrink: 0;
    text-align: center;
  }

  &-actions {
    width: 240px;
    flex-shrink: 0;
    text-align: right;
  }
}

.wf-dept-tree {
  width: 100%;
  padding-right: 4px;

  :deep(.el-tree-node__content) {
    display: flex;
    align-items: center;
    min-height: 40px;
    padding: 4px 8px;
    border-radius: 6px;
  }

  :deep(.el-tree-node__content:hover) {
    background: rgba(45, 140, 240, 0.04);
  }

  :deep(.el-tree-node.is-current > .el-tree-node__content) {
    background: var(--wf-tab-active-bg);
  }

  :deep(.el-tree-node__content .el-tree-node__label) {
    display: none;
  }

  :deep(.el-tree-node__children) {
    margin-left: 8px;
  }

  &__row {
    display: flex;
    align-items: center;
    gap: 8px;
    width: 100%;
    padding: 2px 4px;
    box-sizing: border-box;

    &.is-disabled .wf-dept-tree__name {
      color: var(--wf-text-secondary);
    }
  }

  &__name {
    flex: 1;
    min-width: 0;
    font-weight: 500;
    font-size: 14px;
    color: var(--wf-text);
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  &__code {
    width: 120px;
    flex-shrink: 0;
    font-size: 12px;
    color: var(--wf-text-secondary);
    padding: 2px 8px;
    border-radius: 10px;
    border: 1px solid #e6eef8;
    background: #fafbfc;
    font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    box-sizing: border-box;

    &--empty {
      border-color: transparent;
      background: transparent;
      color: var(--wf-text-muted);
    }
  }

  &__type {
    width: 72px;
    flex-shrink: 0;
    display: flex;
    justify-content: center;
  }

  &__actions {
    display: flex;
    align-items: center;
    gap: 2px;
    flex-shrink: 0;
    width: 240px;
    justify-content: flex-end;
  }
}

@media (max-width: 960px) {
  .wf-dept-tree__head-code,
  .wf-dept-tree__code {
    display: none;
  }
}

@media (max-width: 720px) {
  .wf-dept-page__toolbar {
    flex-direction: column;
    align-items: stretch;
  }

  .wf-dept-page__search {
    width: 100%;
  }

  .wf-dept-page__stats {
    display: none;
  }

  .wf-dept-tree__head-type,
  .wf-dept-tree__head-actions {
    display: none;
  }
}
</style>
