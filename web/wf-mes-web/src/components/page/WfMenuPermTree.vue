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

const pageIdSet = computed(() => {
  const set = new Set<number>()
  const walk = (nodes: MenuItem[]) => {
    for (const node of nodes) {
      if (node.menuType === MENU_PAGE) {
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

/** 仅同步页面节点到 el-tree，避免把目录 id 写入 checkedKeys 触发「假全选」。 */
const treeCheckedKeys = computed(() =>
  props.modelValue.filter((id) => pageIdSet.value.has(id)).map((id) => Number(id))
)

const selectedCount = computed(
  () =>
    props.modelValue.filter((id) => pageIdSet.value.has(id) || buttonIdSet.value.has(id)).length
)

const buttonParentPageMap = computed(() => {
  const map = new Map<number, number>()
  const walk = (nodes: MenuItem[]) => {
    for (const node of nodes) {
      if (node.menuType === MENU_PAGE) {
        for (const child of node.children ?? []) {
          if (child.menuType === MENU_BUTTON) {
            map.set(child.id, node.id)
          }
        }
      }
      if (node.children?.length) {
        walk(node.children)
      }
    }
  }
  walk(props.data)
  return map
})

function collectPageAndButtonIds(nodes: MenuItem[]): number[] {
  const ids: number[] = []
  const walk = (list: MenuItem[]) => {
    for (const node of list) {
      if (node.menuType === MENU_PAGE || node.menuType === MENU_BUTTON) {
        ids.push(node.id)
      }
      if (node.children?.length) {
        walk(node.children)
      }
    }
  }
  walk(nodes)
  return ids
}

function normalizeIds(ids: number[]) {
  return [...new Set(ids.filter((id) => pageIdSet.value.has(id) || buttonIdSet.value.has(id)))]
}

function emitIds(ids: number[]) {
  emit('update:modelValue', normalizeIds(ids))
}

function syncTreeKeys() {
  nextTick(() => {
    treeRef.value?.setCheckedKeys(treeCheckedKeys.value, false)
  })
}

function syncFromTree() {
  // leafOnly=true：只取页面叶子，目录半选不会误当成全选子孙
  const leafKeys = ((treeRef.value?.getCheckedKeys(true) as number[]) ?? []).map(Number)
  const pageIds = new Set(leafKeys.filter((id) => pageIdSet.value.has(id)))
  const next = new Set<number>(pageIds)

  for (const id of props.modelValue) {
    if (!buttonIdSet.value.has(id)) {
      continue
    }
    const parentPageId = buttonParentPageMap.value.get(id)
    if (parentPageId != null && pageIds.has(parentPageId)) {
      next.add(id)
    }
  }

  emitIds([...next])
}

function handleTreeCheck() {
  syncFromTree()
}

function toggleButton(buttonId: number, checked: boolean | string | number) {
  const next = new Set(normalizeIds(props.modelValue))
  if (checked) {
    next.add(buttonId)
    const pageId = buttonParentPageMap.value.get(buttonId)
    if (pageId != null) {
      next.add(pageId)
    }
  } else {
    next.delete(buttonId)
  }
  emitIds([...next])
}

function checkAll() {
  emitIds(collectPageAndButtonIds(props.data))
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
        {{ t('system.role.permSelectedCount', { count: selectedCount }) }}
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
