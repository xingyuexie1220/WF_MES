<script setup lang="ts">
import { computed, nextTick, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import type { MenuItem } from '@/types/system/menu'
import type { TreeInstance } from 'element-plus'

defineOptions({ name: 'WfMenuPermTree' })

const MENU_PAGE = 2
const MENU_BUTTON = 3

const props = defineProps<{
  data: MenuItem[]
  modelValue: number[]
}>()

const emit = defineEmits<{
  'update:modelValue': [value: number[]]
}>()

const { t } = useI18n()
const treeRef = ref<TreeInstance>()
const defaultExpandAll = ref(true)
const treeKey = ref(0)

type PermDisplayNode = MenuItem & { buttons?: MenuItem[] }

function buildDisplayTree(nodes: MenuItem[]): PermDisplayNode[] {
  return nodes
    .filter((node) => node.menuType !== MENU_BUTTON)
    .map((node) => {
      const rawChildren = node.children ?? []
      const buttons =
        node.menuType === MENU_PAGE
          ? rawChildren.filter((child) => child.menuType === MENU_BUTTON)
          : []
      const children = buildDisplayTree(rawChildren.filter((child) => child.menuType !== MENU_BUTTON))
      return {
        ...node,
        buttons: buttons.length ? buttons : undefined,
        children: children.length ? children : undefined
      }
    })
}

const displayTree = computed(() => buildDisplayTree(props.data))

const checkedSet = computed(() => new Set(props.modelValue))

const buttonIdSet = computed(() => {
  const set = new Set<number>()
  const walk = (nodes: MenuItem[]) => {
    for (const node of nodes) {
      if (node.menuType === MENU_BUTTON) {
        set.add(node.id)
      }
      if (node.children?.length) {
        walk(node.children)
      }
    }
  }
  walk(props.data)
  return set
})

const treeCheckedKeys = computed(() => props.modelValue.filter((id) => !buttonIdSet.value.has(id)))

function collectAllIds(nodes: MenuItem[]): number[] {
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

function findDisplayNode(nodes: PermDisplayNode[], id: number): PermDisplayNode | null {
  for (const node of nodes) {
    if (node.id === id) {
      return node
    }
    if (node.children?.length) {
      const found = findDisplayNode(node.children as PermDisplayNode[], id)
      if (found) {
        return found
      }
    }
  }
  return null
}

function collectAncestorIds(nodes: MenuItem[], targetId: number, ancestors: number[] = []): number[] | null {
  for (const node of nodes) {
    if (node.id === targetId) {
      return ancestors
    }
    if (node.children?.length) {
      const found = collectAncestorIds(node.children, targetId, [...ancestors, node.id])
      if (found) {
        return found
      }
    }
  }
  return null
}

function expandWithAncestors(ids: number[]) {
  const result = new Set<number>(ids)
  for (const id of ids) {
    const ancestors = collectAncestorIds(props.data, id)
    ancestors?.forEach((ancestorId) => result.add(ancestorId))
  }
  return [...result]
}

function emitIds(ids: number[]) {
  emit('update:modelValue', expandWithAncestors(ids))
}

function syncTreeKeys() {
  nextTick(() => {
    const keys = treeCheckedKeys.value.map((id) => Number(id))
    treeRef.value?.setCheckedKeys(keys, false)
  })
}

function syncFromTree() {
  const checkedKeys = (treeRef.value?.getCheckedKeys(false) as number[]) ?? []
  const next = new Set<number>(checkedKeys)
  for (const key of checkedKeys) {
    const node = findDisplayNode(displayTree.value, key)
    node?.buttons?.forEach((button) => next.add(button.id))
  }
  emitIds([...next])
}

function handleTreeCheck() {
  syncFromTree()
}

function toggleButton(buttonId: number, checked: boolean | string | number) {
  const next = new Set(props.modelValue)
  if (checked) {
    next.add(buttonId)
  } else {
    next.delete(buttonId)
  }
  emitIds([...next])
}

function checkAll() {
  emitIds(collectAllIds(props.data))
  syncTreeKeys()
}

function clearAll() {
  emitIds([])
  syncTreeKeys()
}

function expandAll() {
  defaultExpandAll.value = true
  treeKey.value += 1
}

function collapseAll() {
  defaultExpandAll.value = false
  treeKey.value += 1
}

watch(() => props.modelValue, syncTreeKeys, { immediate: true, flush: 'post' })
watch(displayTree, syncTreeKeys, { immediate: true, flush: 'post' })
</script>

<template>
  <div class="wf-menu-perm-tree">
    <div class="wf-menu-perm-tree__toolbar">
      <el-button link type="primary" @click="checkAll">{{ t('system.role.permCheckAll') }}</el-button>
      <el-button link type="primary" @click="clearAll">{{ t('system.role.permClearAll') }}</el-button>
      <span class="wf-menu-perm-tree__sep" />
      <el-button link type="primary" @click="expandAll">{{ t('system.role.permExpandAll') }}</el-button>
      <el-button link type="primary" @click="collapseAll">{{ t('system.role.permCollapseAll') }}</el-button>
      <span class="wf-menu-perm-tree__count">
        {{ t('system.role.permSelectedCount', { count: modelValue.length }) }}
      </span>
    </div>
    <div class="wf-menu-perm-tree__body">
      <el-tree
        :key="treeKey"
        ref="treeRef"
        :data="displayTree"
        node-key="id"
        show-checkbox
        :default-expand-all="defaultExpandAll"
        :props="{ label: 'menuName', children: 'children' }"
        @check="handleTreeCheck"
      >
        <template #default="{ data }">
          <div class="wf-menu-perm-tree__node">
            <span class="wf-menu-perm-tree__label">{{ data.menuName }}</span>
            <div v-if="data.buttons?.length" class="wf-menu-perm-tree__buttons">
              <el-checkbox
                v-for="button in data.buttons"
                :key="button.id"
                :model-value="checkedSet.has(button.id)"
                class="wf-menu-perm-tree__btn-check"
                @click.stop
                @change="(val: boolean | string | number) => toggleButton(button.id, val)"
              >
                {{ button.menuName }}
              </el-checkbox>
            </div>
          </div>
        </template>
      </el-tree>
      <el-empty v-if="!displayTree.length" :description="t('system.menu.empty')" :image-size="72" />
    </div>
  </div>
</template>

<style scoped lang="scss">
.wf-menu-perm-tree {
  display: flex;
  flex-direction: column;
  min-height: 0;
  flex: 1;

  &__toolbar {
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    gap: 4px 8px;
    margin-bottom: 8px;
    flex-shrink: 0;
  }

  &__sep {
    width: 1px;
    height: 14px;
    background: #e4e7ed;
  }

  &__count {
    margin-left: auto;
    font-size: 12px;
    color: var(--wf-text-muted);
  }

  &__body {
    flex: 1;
    min-height: 360px;
    overflow: auto;
    border: 1px solid #ebeef5;
    border-radius: 6px;
    padding: 10px 12px;
    background: #fff;
  }

  &__node {
    display: flex;
    flex-direction: column;
    align-items: flex-start;
    gap: 6px;
    min-height: 26px;
    padding: 2px 8px 4px 0;
  }

  &__label {
    font-size: 13px;
    font-weight: 500;
    color: var(--wf-text);
    line-height: 22px;
  }

  &__buttons {
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    gap: 6px 14px;
    margin-left: 2px;
    padding: 6px 0 2px 14px;
    border-left: 2px dashed #dcdfe6;
  }

  &__btn-check {
    margin-right: 0;

    :deep(.el-checkbox__label) {
      font-size: 12px;
      color: var(--wf-text-secondary);
      padding-left: 6px;
    }
  }

  :deep(.el-tree) {
    background: transparent;
  }

  :deep(.el-tree-node__content) {
    height: auto;
    min-height: 30px;
    align-items: flex-start;
    padding-top: 2px;
    padding-bottom: 2px;
  }
}
</style>
