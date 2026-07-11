<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { useTabsStore } from '@/stores/layout/tabs'

const { t } = useI18n()
const tabsStore = useTabsStore()
const tabsRef = ref<{ $el: HTMLElement } | null>(null)

const contextMenuVisible = ref(false)
const contextMenuLeft = ref(0)
const contextMenuTop = ref(0)
const contextTargetPath = ref('/dashboard')

const activeTab = computed({
  get: () => tabsStore.activePath,
  set: (path: string) => tabsStore.setActive(path)
})

const contextActions = computed(() => {
  const path = contextTargetPath.value
  const index = tabsStore.tabs.findIndex((tab) => tab.path === path)
  const tab = tabsStore.tabs[index]
  const canClose = tab?.closable !== false

  return {
    left: canClose && index > 0,
    right: canClose && index >= 0 && index < tabsStore.tabs.length - 1,
    other: canClose && tabsStore.tabs.length > 1,
    current: canClose
  }
})

function handleClose(name: string | number) {
  tabsStore.removeTab(String(name))
}

function openContextMenu(event: Event) {
  const mouseEvent = event as MouseEvent
  const target = (mouseEvent.target as HTMLElement | null)?.closest('.el-tabs__item')
  if (!target) {
    return
  }
  event.preventDefault()
  const id = target.getAttribute('id') || ''
  const matched = id.match(/tab-(.+)$/)
  if (!matched) {
    return
  }
  contextTargetPath.value = decodeURIComponent(matched[1])
  contextMenuLeft.value = mouseEvent.clientX
  contextMenuTop.value = mouseEvent.clientY
  contextMenuVisible.value = true
}

function hideContextMenu() {
  contextMenuVisible.value = false
}

function runContextAction(action: 'left' | 'right' | 'other' | 'current' | 'all' | 'refresh') {
  const path = contextTargetPath.value
  switch (action) {
    case 'left':
      tabsStore.closeLeft(path)
      break
    case 'right':
      tabsStore.closeRight(path)
      break
    case 'other':
      tabsStore.closeOthers(path)
      break
    case 'current':
      tabsStore.removeTab(path)
      break
    case 'all':
      tabsStore.closeAll()
      break
    case 'refresh':
      tabsStore.setActive(path)
      tabsStore.refreshCurrentView(path)
      break
  }
  hideContextMenu()
}

function handleMoreCommand(command: string) {
  switch (command) {
    case 'closeCurrent':
      tabsStore.removeTab(tabsStore.activePath)
      break
    case 'closeLeft':
      tabsStore.closeLeft(tabsStore.activePath)
      break
    case 'closeRight':
      tabsStore.closeRight(tabsStore.activePath)
      break
    case 'closeOthers':
      tabsStore.closeOthers(tabsStore.activePath)
      break
    case 'closeAll':
      tabsStore.closeAll()
      break
    case 'refresh':
      tabsStore.refreshCurrentView()
      break
    default:
      tabsStore.setActive(command)
  }
}

onMounted(() => {
  const nav = tabsRef.value?.$el.querySelector('.el-tabs__nav')
  nav?.addEventListener('contextmenu', openContextMenu)
  document.addEventListener('click', hideContextMenu)
})

onUnmounted(() => {
  const nav = tabsRef.value?.$el.querySelector('.el-tabs__nav')
  nav?.removeEventListener('contextmenu', openContextMenu)
  document.removeEventListener('click', hideContextMenu)
})
</script>

<template>
  <div class="wf-tags">
    <el-tabs
      ref="tabsRef"
      v-model="activeTab"
      class="wf-tags__tabs"
      type="border-card"
      @tab-remove="handleClose"
    >
      <el-tab-pane
        v-for="tab in tabsStore.tabs"
        :key="tab.path"
        :label="tabsStore.getTabLabel(tab, t)"
        :name="tab.path"
        :closable="tab.closable !== false"
      />
    </el-tabs>

    <el-dropdown class="wf-tags__more" trigger="click" @command="handleMoreCommand">
      <el-button size="small">{{ t('common.more') }}</el-button>
      <template #dropdown>
        <el-dropdown-menu>
          <el-dropdown-item command="closeCurrent">{{ t('layout.closeCurrent') }}</el-dropdown-item>
          <el-dropdown-item command="closeLeft">{{ t('layout.closeLeft') }}</el-dropdown-item>
          <el-dropdown-item command="closeRight">{{ t('layout.closeRight') }}</el-dropdown-item>
          <el-dropdown-item command="closeOthers">{{ t('layout.closeOthers') }}</el-dropdown-item>
          <el-dropdown-item command="closeAll">{{ t('layout.closeAll') }}</el-dropdown-item>
          <el-dropdown-item divided command="refresh">{{ t('layout.refreshPage') }}</el-dropdown-item>
          <el-dropdown-item
            v-for="tab in tabsStore.tabs"
            :key="tab.path"
            :command="tab.path"
          >
            {{ tabsStore.getTabLabel(tab, t) }}
          </el-dropdown-item>
        </el-dropdown-menu>
      </template>
    </el-dropdown>

    <ul
      v-show="contextMenuVisible"
      class="wf-tags__context-menu"
      :style="{ left: `${contextMenuLeft}px`, top: `${contextMenuTop}px` }"
      @click.stop
    >
      <li v-if="contextActions.current" @click="runContextAction('current')">{{ t('layout.closeCurrent') }}</li>
      <li v-if="contextActions.left" @click="runContextAction('left')">{{ t('layout.closeLeft') }}</li>
      <li v-if="contextActions.right" @click="runContextAction('right')">{{ t('layout.closeRight') }}</li>
      <li v-if="contextActions.other" @click="runContextAction('other')">{{ t('layout.closeOthers') }}</li>
      <li @click="runContextAction('all')">{{ t('layout.closeAll') }}</li>
      <li @click="runContextAction('refresh')">{{ t('layout.refreshPage') }}</li>
    </ul>
  </div>
</template>

<style scoped lang="scss">
.wf-tags {
  position: relative;
  display: flex;
  align-items: stretch;
  height: var(--wf-tabs-height);
  background: #fff;
  border-bottom: 1px solid var(--wf-border);
  border-left: 1px solid var(--wf-border);
  flex-shrink: 0;
  margin-top: -1px;
  box-sizing: border-box;

  &__tabs {
    flex: 1;
    min-width: 0;
    --el-tabs-header-height: var(--wf-tabs-height);

    :deep(.el-tabs--border-card) {
      border: none;
      box-shadow: none;
      background: transparent;
    }

    :deep(.el-tabs__header) {
      margin: 0;
      background: #fff !important;
      border: none;
      height: 100%;
    }

    :deep(.el-tabs__nav-wrap),
    :deep(.el-tabs__nav-scroll),
    :deep(.el-tabs__nav) {
      height: 100% !important;
    }

    :deep(.el-tabs__nav-wrap) {
      margin-bottom: 0;
    }

    :deep(.el-tabs__nav) {
      align-items: stretch;
    }

    :deep(.el-tabs__content) {
      display: none;
    }

    :deep(.el-tabs__item) {
      height: 100%;
      line-height: var(--wf-tabs-height);
      padding: 0 12px;
      margin-right: 4px;
      border: none !important;
      border-radius: 4px;
      color: var(--wf-tab-menu-text);
      font-size: 14px;
      transition:
        color 0.2s ease,
        background-color 0.2s ease;

      &:hover:not(.is-active) {
        color: var(--wf-tab-primary) !important;
        background: #f5f6f8 !important;
      }

      &.is-active {
        color: var(--wf-tab-primary) !important;
        background: var(--wf-tab-active-bg) !important;
        font-weight: 500;
      }

      .is-icon-close:hover {
        color: #fff !important;
        background-color: var(--wf-tab-primary) !important;
      }
    }
  }

  &__more {
    flex-shrink: 0;
    align-self: center;
    margin: 0 8px;
  }

  &__context-menu {
    position: fixed;
    z-index: 3000;
    width: 132px;
    margin: 0;
    padding: 5px 0;
    list-style: none;
    background: #fff;
    border: 1px solid #eaeaea;
    border-radius: 4px;
    box-shadow: 2px 2px 3px rgba(182, 182, 182, 0.2);
    font-size: 14px;
    color: #333;

    li {
      padding: 7px 12px;
      cursor: pointer;

      &:hover {
        background: #fafafa;
      }
    }
  }
}
</style>
