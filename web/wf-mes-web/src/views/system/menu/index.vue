<script setup lang="ts">
defineOptions({ name: 'SystemMenu' })
import { computed, onMounted, reactive, ref, watch, type Component } from 'vue'
import { useI18n } from 'vue-i18n'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { FormInstance, FormRules, TableInstance } from 'element-plus'
import {
  CirclePlusFilled,
  Delete,
  EditPen,
  FolderOpened,
  Key,
  Menu as MenuIcon,
  Plus,
  Search
} from '@element-plus/icons-vue'
import { useUserStore } from '@/stores/auth/user'
import { createMenuApi, deleteMenuApi, getMenuTreeApi, updateMenuApi } from '@/api/system/menu'
import {
  WfDialogFooter,
  WfMenuIconPicker,
  WfMenuTypeTag,
  WfPage,
  WfPageBody,
  WfStatusTag,
  WfTable
} from '@/components/page'
import { CLIENT_DESKTOP, CLIENT_MOBILE, CLIENT_WEB } from '@/types/common/enums'
import type { MenuItem } from '@/types/system/menu'
import { resolveMenuTitleKey } from '@/config/menuTitleKeyMap'
import { resolveMenuDisplayName } from '@/utils/menu'
import { resolveMenuIcon } from '@/utils/menu'

const MENU_DIR = 1
const MENU_PAGE = 2
const MENU_BUTTON = 3

const { t, tm } = useI18n()
const userStore = useUserStore()
const formRef = ref<FormInstance>()
const tableRef = ref<{ tableRef?: TableInstance }>()
const loading = ref(false)
const submitting = ref(false)
const deletingId = ref<number | null>(null)
const dialogVisible = ref(false)
const editingId = ref<number | null>(null)
const rawTreeData = ref<MenuItem[]>([])

const filter = reactive({
  menuName: '',
  menuType: undefined as number | undefined,
  status: undefined as number | undefined
})

const form = reactive({
  parentId: 0,
  menuName: '',
  i18nKey: '',
  menuType: MENU_PAGE,
  clientType: CLIENT_WEB,
  path: '',
  component: '',
  permission: '',
  icon: '',
  sort: 1,
  visible: true,
  status: 1,
  remark: ''
})

const canAdd = computed(() => userStore.hasPermission('system:menu:add'))
const canEdit = computed(() => userStore.hasPermission('system:menu:edit'))
const canDelete = computed(() => userStore.hasPermission('system:menu:delete'))

const menuTypeLabels = computed(() => tm('system.menu.typeLabel') as string[])

const isFormMobileClient = computed(() => form.clientType === CLIENT_MOBILE)
const isFormDesktopClient = computed(() => form.clientType === CLIENT_DESKTOP)

const formComponentLabel = computed(() => {
  if (isFormMobileClient.value) return t('system.menu.pagePath')
  if (isFormDesktopClient.value) return t('system.menu.viewName')
  return t('system.menu.component')
})

const formPathPlaceholder = computed(() =>
  isFormMobileClient.value ? t('system.menu.pathPlaceholderMobile') : t('system.menu.pathPlaceholderWeb')
)

const formComponentPlaceholder = computed(() =>
  isFormMobileClient.value ? t('system.menu.pagePathPlaceholder') : t('system.menu.componentPlaceholder')
)

const formMenuTypeOptions = computed(() => {
  const allowed = getAllowedMenuTypes(form.parentId)
  return [
    { label: t('system.menu.typeDir'), value: MENU_DIR },
    { label: t('system.menu.typeMenu'), value: MENU_PAGE },
    { label: t('system.menu.typeButton'), value: MENU_BUTTON }
  ].filter((option) => allowed.includes(option.value))
})

const parentTreeOptions = computed(() => {
  const clientRoots = rawTreeData.value.filter((node) => node.clientType === form.clientType)
  return [
    {
      id: 0,
      menuName: t('system.menu.rootMenu'),
      children: filterTreeForParent(clientRoots, editingId.value)
    }
  ]
})

const filteredTableData = computed(() =>
  filterMenuTree(rawTreeData.value, filter.menuName.trim(), filter.menuType, filter.status)
)

const menuNodeCount = computed(() => countMenuNodes(filteredTableData.value))

const formRules = computed<FormRules>(() => {
  const rules: FormRules = {
    menuName: [{ required: true, message: t('system.menu.menuName'), trigger: 'blur' }]
  }
  if (form.menuType === MENU_PAGE) {
    rules.path = [{ required: true, message: t('system.menu.path'), trigger: 'blur' }]
    if (!isFormMobileClient.value) {
      rules.component = [{ required: true, message: t('system.menu.component'), trigger: 'blur' }]
    }
  }
  if (form.menuType === MENU_BUTTON) {
    rules.permission = [{ required: true, message: t('system.menu.permission'), trigger: 'blur' }]
  }
  return rules
})

function countMenuNodes(nodes: MenuItem[]): number {
  let count = 0
  const walk = (list: MenuItem[]) => {
    for (const node of list) {
      count += 1
      if (node.children?.length) {
        walk(node.children)
      }
    }
  }
  walk(nodes)
  return count
}

function filterTreeForParent(nodes: MenuItem[], excludeId?: number | null): MenuItem[] {
  return nodes
    .filter((node) => node.menuType !== MENU_BUTTON && node.id !== excludeId)
    .map((node) => ({
      ...node,
      children: node.children?.length ? filterTreeForParent(node.children, excludeId) : []
    }))
}

function resolveMenuTitleKeyForRow(row: MenuItem): string | undefined {
  return row.i18nKey || resolveMenuTitleKey(row.path)
}

function displayMenuName(row: MenuItem): string {
  return resolveMenuDisplayName(row.menuName, row.i18nKey, row.path, t)
}

function menuNameMatches(row: MenuItem, keyword: string): boolean {
  if (!keyword) {
    return true
  }
  const lower = keyword.toLowerCase()
  if (row.menuName.toLowerCase().includes(lower)) {
    return true
  }
  const titleKey = resolveMenuTitleKeyForRow(row)
  if (titleKey && t(titleKey).toLowerCase().includes(lower)) {
    return true
  }
  if (row.i18nKey?.toLowerCase().includes(lower)) {
    return true
  }
  return false
}

function filterMenuTree(
  nodes: MenuItem[],
  keyword: string,
  menuType: number | undefined,
  status: number | undefined
): MenuItem[] {
  const result: MenuItem[] = []
  for (const node of nodes) {
    const children = node.children?.length
      ? filterMenuTree(node.children, keyword, menuType, status)
      : []
    const nameMatch = menuNameMatches(node, keyword)
    const typeMatch = menuType === undefined || node.menuType === menuType
    const statusMatch = status === undefined || node.status === status
    const selfMatch = nameMatch && typeMatch && statusMatch
    if (selfMatch || children.length > 0) {
      result.push({ ...node, children })
    }
  }
  return result
}

function findMenuNode(nodes: MenuItem[], id: number): MenuItem | null {
  for (const node of nodes) {
    if (node.id === id) {
      return node
    }
    if (node.children?.length) {
      const found = findMenuNode(node.children, id)
      if (found) {
        return found
      }
    }
  }
  return null
}

function getAllowedMenuTypes(parentId: number): number[] {
  if (parentId <= 0) {
    return [MENU_DIR]
  }
  const parent = findMenuNode(rawTreeData.value, parentId)
  if (parent?.menuType === MENU_PAGE) {
    return [MENU_BUTTON]
  }
  if (parent?.menuType === MENU_DIR) {
    return [MENU_DIR, MENU_PAGE]
  }
  return [MENU_DIR, MENU_PAGE]
}

function resolveDefaultMenuType(parentId: number): number {
  if (parentId <= 0) {
    return MENU_DIR
  }
  const parent = findMenuNode(rawTreeData.value, parentId)
  if (parent?.menuType === MENU_PAGE) {
    return MENU_BUTTON
  }
  return MENU_PAGE
}

function applyMenuTypeConstraint() {
  const allowed = getAllowedMenuTypes(form.parentId)
  if (!allowed.includes(form.menuType)) {
    form.menuType = allowed[0] ?? MENU_PAGE
  }
}

function clientLabel(clientType: number) {
  if (clientType === CLIENT_MOBILE) return t('system.menu.clientMobile')
  if (clientType === CLIENT_DESKTOP) return t('system.menu.clientDesktop')
  return t('system.menu.clientWeb')
}

function showRowIcon(row: MenuItem) {
  return row.clientType === CLIENT_WEB && row.menuType !== MENU_BUTTON
}

function resolveRowIcon(row: MenuItem): Component {
  if (row.menuType === MENU_BUTTON) {
    return Key
  }
  if (row.icon?.trim()) {
    return resolveMenuIcon(row.icon)
  }
  if (row.menuType === MENU_DIR) {
    return FolderOpened
  }
  return MenuIcon
}

function menuNameIconClass(row: MenuItem) {
  if (row.menuType === MENU_BUTTON) {
    return 'is-btn'
  }
  if (row.icon?.trim()) {
    return 'is-configured'
  }
  if (row.menuType === MENU_DIR) {
    return 'is-dir'
  }
  return 'is-page'
}

function walkTreeParents(nodes: MenuItem[], fn: (row: MenuItem) => void) {
  for (const node of nodes) {
    if (node.children?.length) {
      fn(node)
      walkTreeParents(node.children, fn)
    }
  }
}

function setTreeExpand(expand: boolean) {
  const table = tableRef.value?.tableRef
  if (!table) {
    return
  }
  walkTreeParents(filteredTableData.value, (row) => {
    table.toggleRowExpansion(row, expand)
  })
}

function showClientTag(row: MenuItem) {
  return row.parentId === 0
}

function displayCell(value?: string | null) {
  return value?.trim() || ''
}

function displayRouteCell(row: MenuItem) {
  if (row.menuType === MENU_BUTTON) {
    return ''
  }
  return displayCell(row.path)
}

function displayComponentCell(row: MenuItem) {
  if (row.menuType !== MENU_PAGE) {
    return ''
  }
  return displayCell(row.component)
}

function displayPermissionCell(row: MenuItem) {
  if (row.menuType === MENU_DIR) {
    return ''
  }
  return displayCell(row.permission)
}

async function loadData() {
  loading.value = true
  try {
    rawTreeData.value = await getMenuTreeApi()
  } finally {
    loading.value = false
  }
}

function handleReset() {
  filter.menuName = ''
  filter.menuType = undefined
  filter.status = undefined
}

function resolveClientType(parentId: number) {
  if (parentId <= 0) {
    return CLIENT_WEB
  }
  return findMenuNode(rawTreeData.value, parentId)?.clientType ?? CLIENT_WEB
}

function resetForm(parentId = 0) {
  const menuType = resolveDefaultMenuType(parentId)
  Object.assign(form, {
    parentId,
    menuName: '',
    i18nKey: '',
    menuType,
    clientType: resolveClientType(parentId),
    path: '',
    component: '',
    permission: '',
    icon: '',
    sort: 1,
    visible: menuType !== MENU_BUTTON,
    status: 1,
    remark: ''
  })
}

function openCreate(parentId = 0) {
  editingId.value = null
  resetForm(parentId)
  dialogVisible.value = true
}

function openEdit(row: MenuItem) {
  editingId.value = row.id
  Object.assign(form, {
    parentId: row.parentId,
    menuName: row.menuName,
    i18nKey: row.i18nKey ?? '',
    menuType: row.menuType,
    clientType: row.clientType,
    path: row.path ?? '',
    component: row.component ?? '',
    permission: row.permission ?? '',
    icon: row.icon ?? '',
    sort: row.sort,
    visible: row.visible,
    status: row.status,
    remark: row.remark ?? ''
  })
  dialogVisible.value = true
}

async function submitForm() {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return

  submitting.value = true
  try {
    const payload = { ...form }
    if (editingId.value) {
      await updateMenuApi(editingId.value, payload)
      ElMessage.success(t('common.updateSuccess'))
    } else {
      await createMenuApi(payload)
      ElMessage.success(t('common.createSuccess'))
    }
    dialogVisible.value = false
    await loadData()
  } finally {
    submitting.value = false
  }
}

async function handleDelete(row: MenuItem) {
  if (row.children?.length) {
    ElMessage.warning(t('system.menu.hasChildren'))
    return
  }
  try {
    await ElMessageBox.confirm(t('system.menu.confirmDelete', { name: row.menuName }), t('common.tip'), {
      type: 'warning',
      confirmButtonText: t('common.confirm'),
      cancelButtonText: t('common.cancel')
    })
    deletingId.value = row.id
    try {
      await deleteMenuApi(row.id)
      ElMessage.success(t('common.deleteSuccess'))
      await loadData()
    } finally {
      deletingId.value = null
    }
  } catch {
    /* cancelled */
  }
}

function canAddChild(row: MenuItem) {
  return row.menuType !== MENU_BUTTON
}

watch(
  () => form.parentId,
  (parentId) => {
    if (parentId > 0) {
      form.clientType = resolveClientType(parentId)
    }
    applyMenuTypeConstraint()
  }
)

watch(
  () => form.menuType,
  (menuType, prev) => {
    if (menuType === MENU_BUTTON) {
      form.visible = false
      form.icon = ''
      form.path = ''
      form.component = ''
    } else if (prev === MENU_BUTTON) {
      form.visible = true
    }
  }
)

onMounted(loadData)
</script>

<template>
  <WfPage class="wf-menu-page">
    <WfPageBody class="wf-menu-page__body">
      <div class="wf-list-panel">
        <div class="wf-list-panel__head">
          <div class="wf-list-panel__title-block">
            <div class="wf-list-panel__title">
              <el-icon class="wf-list-panel__title-icon"><MenuIcon /></el-icon>
              <span>{{ t('system.menu.title') }}</span>
              <span class="wf-list-panel__count">{{ t('system.menu.nodeCount', { count: menuNodeCount }) }}</span>
            </div>
            <p class="wf-list-panel__desc">{{ t('system.menu.pageDesc') }}</p>
          </div>
          <div class="wf-list-panel__toolbar">
            <div class="wf-list-panel__group wf-list-panel__group--filter">
              <el-input
                v-model="filter.menuName"
                clearable
                :prefix-icon="Search"
                :placeholder="t('system.menu.searchMenuName')"
                class="wf-list-panel__input wf-list-panel__input--wide"
              />
              <el-select
                v-model="filter.menuType"
                clearable
                :placeholder="t('system.menu.menuType')"
                class="wf-list-panel__select"
              >
                <el-option :label="t('system.menu.typeDir')" :value="MENU_DIR" />
                <el-option :label="t('system.menu.typeMenu')" :value="MENU_PAGE" />
                <el-option :label="t('system.menu.typeButton')" :value="MENU_BUTTON" />
              </el-select>
              <el-select
                v-model="filter.status"
                clearable
                :placeholder="t('common.status')"
                class="wf-list-panel__select"
              >
                <el-option :label="t('common.enabled')" :value="1" />
                <el-option :label="t('common.disabled')" :value="0" />
              </el-select>
            </div>
            <span class="wf-list-panel__divider" aria-hidden="true" />
            <div class="wf-list-panel__group wf-list-panel__group--action">
              <el-button link type="primary" @click="setTreeExpand(true)">{{ t('system.menu.expandAll') }}</el-button>
              <el-button link type="primary" @click="setTreeExpand(false)">{{ t('system.menu.collapseAll') }}</el-button>
              <el-button v-if="canAdd" type="success" :icon="CirclePlusFilled" @click="openCreate(0)">
                {{ t('system.menu.add') }}
              </el-button>
              <el-button @click="handleReset">{{ t('common.reset') }}</el-button>
            </div>
          </div>
        </div>

        <WfTable
          ref="tableRef"
          v-loading="loading"
          auto-height
          class="wf-menu-table"
          :data="filteredTableData"
          row-key="id"
          default-expand-all
          :tree-props="{ children: 'children' }"
        >
          <el-table-column :label="t('system.menu.menuName')" min-width="168" show-overflow-tooltip tree-node>
            <template #default="{ row }">
              <span class="wf-menu-name" :class="{ 'is-button': row.menuType === MENU_BUTTON }">
                <el-icon v-if="showRowIcon(row)" class="wf-menu-name__icon" :class="menuNameIconClass(row)">
                  <component :is="resolveRowIcon(row)" />
                </el-icon>
                {{ displayMenuName(row) }}
              </span>
            </template>
          </el-table-column>
          <el-table-column :label="t('system.menu.clientType')" width="88" align="center">
            <template #default="{ row }">
              <span
                v-if="showClientTag(row)"
                class="wf-menu-client"
                :class="row.clientType === CLIENT_MOBILE ? 'is-mobile' : row.clientType === CLIENT_DESKTOP ? 'is-desktop' : 'is-web'"
              >
                {{ clientLabel(row.clientType) }}
              </span>
            </template>
          </el-table-column>
          <el-table-column :label="t('system.menu.menuType')" width="88" align="center">
            <template #default="{ row }">
              <WfMenuTypeTag :type="row.menuType" :label="menuTypeLabels[row.menuType] || ''" />
            </template>
          </el-table-column>
          <el-table-column :label="t('system.menu.path')" min-width="128" show-overflow-tooltip>
            <template #default="{ row }">{{ displayRouteCell(row) }}</template>
          </el-table-column>
          <el-table-column :label="t('system.menu.componentOrPage')" min-width="132" show-overflow-tooltip>
            <template #default="{ row }">{{ displayComponentCell(row) }}</template>
          </el-table-column>
          <el-table-column :label="t('system.menu.permission')" min-width="140" show-overflow-tooltip>
            <template #default="{ row }">
              <el-tag v-if="displayPermissionCell(row)" size="small" type="info" disable-transitions class="wf-menu-perm-tag">
                {{ displayPermissionCell(row) }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column :label="t('common.sort')" width="64" align="center" prop="sort" />
          <el-table-column :label="t('common.status')" width="76" align="center">
            <template #default="{ row }">
              <WfStatusTag :status="row.status" />
            </template>
          </el-table-column>
          <el-table-column
            :label="t('common.actions')"
            width="120"
            align="center"
            class-name="wf-menu-actions-cell"
          >
            <template #default="{ row }">
              <div class="wf-menu-actions">
                <el-tooltip v-if="canAdd && canAddChild(row)" :content="t('system.menu.addChild')" placement="top">
                  <el-button class="wf-menu-actions__btn is-add" text @click="openCreate(row.id)">
                    <el-icon><Plus /></el-icon>
                  </el-button>
                </el-tooltip>
                <el-tooltip v-if="canEdit" :content="t('common.edit')" placement="top">
                  <el-button class="wf-menu-actions__btn is-edit" text @click="openEdit(row)">
                    <el-icon><EditPen /></el-icon>
                  </el-button>
                </el-tooltip>
                <el-tooltip v-if="canDelete" :content="t('common.delete')" placement="top">
                  <el-button
                    class="wf-menu-actions__btn is-delete"
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
            <el-empty :description="t('system.menu.empty')" :image-size="96" />
          </template>
        </WfTable>
      </div>
    </WfPageBody>

    <el-dialog
      v-model="dialogVisible"
      :title="editingId ? t('system.menu.edit') : t('system.menu.add')"
      width="720px"
      destroy-on-close
    >
      <el-form ref="formRef" :model="form" :rules="formRules" label-width="96px">
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item :label="t('system.menu.parentMenu')">
              <el-tree-select
                v-model="form.parentId"
                :data="parentTreeOptions"
                node-key="id"
                :props="{ label: 'menuName', value: 'id', children: 'children' }"
                check-strictly
                :render-after-expand="false"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item :label="t('system.menu.menuType')">
              <el-select
                v-model="form.menuType"
                style="width: 100%"
                :disabled="formMenuTypeOptions.length <= 1"
              >
                <el-option
                  v-for="option in formMenuTypeOptions"
                  :key="option.value"
                  :label="option.label"
                  :value="option.value"
                />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item :label="t('system.menu.menuName')" prop="menuName">
              <el-input v-model="form.menuName" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item :label="t('system.menu.i18nKey')">
              <el-input v-model="form.i18nKey" :placeholder="t('system.menu.i18nKeyPlaceholder')" />
            </el-form-item>
          </el-col>
          <el-col v-if="!isFormMobileClient && form.menuType !== MENU_BUTTON" :span="12">
            <el-form-item :label="t('system.menu.icon')">
              <WfMenuIconPicker v-model="form.icon" />
            </el-form-item>
          </el-col>
          <el-col v-if="form.parentId === 0 && !editingId" :span="12">
            <el-form-item :label="t('system.menu.clientType')">
              <el-radio-group v-model="form.clientType">
                <el-radio :value="CLIENT_WEB">{{ t('system.menu.clientWeb') }}</el-radio>
                <el-radio :value="CLIENT_MOBILE">{{ t('system.menu.clientMobile') }}</el-radio>
                <el-radio :value="CLIENT_DESKTOP">{{ t('system.menu.clientDesktop') }}</el-radio>
              </el-radio-group>
            </el-form-item>
          </el-col>
          <el-col v-if="form.parentId === 0 && editingId" :span="12">
            <el-form-item :label="t('system.menu.clientType')">
              <span class="wf-menu-form-readonly">{{ clientLabel(form.clientType) }}</span>
            </el-form-item>
          </el-col>
          <el-col v-if="form.menuType !== MENU_BUTTON" :span="12">
            <el-form-item :label="t('system.menu.path')" prop="path">
              <el-input v-model="form.path" :placeholder="formPathPlaceholder" />
            </el-form-item>
          </el-col>
          <el-col v-if="form.menuType === MENU_PAGE" :span="12">
            <el-form-item :label="formComponentLabel" prop="component">
              <el-input v-model="form.component" :placeholder="formComponentPlaceholder" />
            </el-form-item>
          </el-col>
          <el-col v-if="form.menuType !== MENU_DIR" :span="12">
            <el-form-item :label="t('system.menu.permission')" prop="permission">
              <el-input v-model="form.permission" :placeholder="t('system.menu.permissionPlaceholder')" />
              <p class="wf-menu-form-hint">{{ t('system.menu.permissionHint') }}</p>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item :label="t('common.sort')">
              <el-input-number v-model="form.sort" :min="0" style="width: 100%" />
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
          <el-col v-if="form.menuType !== MENU_BUTTON" :span="12">
            <el-form-item :label="t('system.menu.visible')">
              <el-switch v-model="form.visible" />
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item :label="t('common.remark')">
              <el-input v-model="form.remark" type="textarea" :rows="2" />
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
.wf-menu-page {
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

.wf-menu-client {
  display: inline-block;
  padding: 0 6px;
  height: 22px;
  line-height: 22px;
  border-radius: 4px;
  font-size: 12px;
  white-space: nowrap;

  &.is-web {
    color: #2d8cf0;
    background: rgba(45, 140, 240, 0.08);
  }

  &.is-mobile {
    color: #52a06b;
    background: rgba(82, 160, 107, 0.08);
  }

  &.is-desktop {
    color: #e6a23c;
    background: rgba(230, 162, 60, 0.1);
  }
}

.wf-menu-name {
  display: inline-flex;
  align-items: center;
  gap: 6px;

  &.is-button {
    font-size: 12px;
    color: var(--wf-text-secondary);
  }

  &__icon {
    font-size: 15px;
    flex-shrink: 0;

    &.is-dir {
      color: #2d8cf0;
    }

    &.is-page {
      color: #52a06b;
    }

    &.is-configured {
      color: var(--wf-primary);
    }

    &.is-btn {
      color: #909399;
    }
  }
}

.wf-menu-perm-tag {
  max-width: 100%;
  font-family: ui-monospace, monospace;
  font-size: 11px;
}

.wf-menu-form-readonly {
  color: var(--wf-text);
  line-height: 32px;
}

.wf-menu-form-hint {
  margin: 4px 0 0;
  font-size: 12px;
  line-height: 1.5;
  color: var(--wf-text-muted);
}

.wf-menu-table {
  :deep(.wf-menu-actions-cell.el-table__cell) {
    overflow: visible;
    text-overflow: clip;
  }

  :deep(.wf-menu-actions-cell .cell) {
    overflow: visible !important;
    text-overflow: clip !important;
    white-space: nowrap;
    padding-right: 12px;
  }
}

.wf-menu-actions {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 2px;

  &__btn {
    flex-shrink: 0;
    width: 28px;
    height: 28px;
    padding: 0;
    border-radius: 4px;
    font-size: 16px;

    &.is-add {
      color: var(--wf-primary);
    }

    &.is-edit {
      color: #e6a23c;
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
