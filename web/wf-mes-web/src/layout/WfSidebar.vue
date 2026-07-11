<script setup lang="ts">
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { ElMessage } from 'element-plus'
import { Search } from '@element-plus/icons-vue'
import { useMenuStore } from '@/stores/layout/menu'
import { filterMenuTree, resolveMenuLabel } from '@/utils/menu'
import WfSidebarMenu from './WfSidebarMenu.vue'
import WfBrand from '@/components/WfBrand.vue'

const route = useRoute()
const router = useRouter()
const menuStore = useMenuStore()
const { t } = useI18n()

const activeMenu = computed(() => route.path)
const sidebarWidth = computed(() =>
  menuStore.collapsed ? 'var(--wf-sidebar-collapsed)' : 'var(--wf-sidebar-width)'
)
const visibleMenus = computed(() =>
  filterMenuTree(menuStore.rawMenuTree, menuStore.keyword, (item) => resolveMenuLabel(item, t))
)

function handleMenuSelect(index: string) {
  if (index.startsWith('placeholder:')) {
    ElMessage.info(t('layout.developing'))
    return
  }
  if (index.startsWith('http://') || index.startsWith('https://')) {
    window.open(index, '_blank')
    return
  }
  router.push(index)
}
</script>

<template>
  <aside class="wf-sidebar" :style="{ width: sidebarWidth }">
    <div class="wf-sidebar__brand" :class="{ 'is-collapsed': menuStore.collapsed }">
      <WfBrand :mini="menuStore.collapsed" :show-text="!menuStore.collapsed" />
    </div>

    <div v-if="!menuStore.collapsed" class="wf-sidebar__search">
      <el-input
        :model-value="menuStore.keyword"
        :placeholder="t('common.searchMenu')"
        :prefix-icon="Search"
        clearable
        @update:model-value="menuStore.setKeyword"
      />
    </div>

    <el-scrollbar class="wf-sidebar__scroll">
      <el-menu
        class="wf-sidebar__menu"
        :default-openeds="menuStore.defaultOpeneds"
        :default-active="activeMenu"
        :collapse="menuStore.collapsed"
        :collapse-transition="false"
        @select="handleMenuSelect"
      >
        <WfSidebarMenu :items="visibleMenus" />
      </el-menu>
    </el-scrollbar>
  </aside>
</template>

<style scoped lang="scss">
.wf-sidebar {
  height: 100%;
  display: flex;
  flex-direction: column;
  background: #fff;
  transition: width 0.2s ease;
  overflow: hidden;
  flex-shrink: 0;

  &__brand {
    height: var(--wf-sidebar-brand-height);
    background: var(--wf-header-blue);
    display: flex;
    align-items: center;
    justify-content: flex-start;
    padding: 0 16px;
    gap: 8px;
    flex-shrink: 0;
    box-sizing: border-box;

    &.is-collapsed {
      justify-content: center;
      padding: 0;
    }
  }

  &__search {
    padding: 10px 12px 0;
  }

  &__scroll {
    flex: 1;
    min-height: 0;
  }

  &__menu {
    border-right: none !important;
    --el-menu-text-color: #000;
    --el-menu-hover-text-color: var(--wf-tab-primary);
    --el-menu-active-color: var(--wf-tab-primary);
    --el-menu-hover-bg-color: var(--wf-tab-active-bg);
    --el-menu-bg-color: #fff;

    :deep(.el-sub-menu.is-opened) {
      background: var(--wf-tab-active-bg) !important;

      .el-menu-item {
        background: rgba(100, 108, 255, 0.04) !important;
      }
    }

    :deep(.el-menu-item.is-active) {
      color: var(--wf-tab-primary) !important;
      background: var(--wf-tab-active-bg) !important;
      position: relative;
      border-left: none !important;

      &::after {
        content: '';
        position: absolute;
        right: 0;
        top: 0;
        width: 2px;
        height: 100%;
        background: var(--wf-tab-primary);
      }
    }

    :deep(.el-menu-item.is-disabled) {
      opacity: 0.55;
      cursor: not-allowed;
    }

    :deep(.wf-menu-item--placeholder) {
      .el-menu-item__title {
        display: inline-flex;
        align-items: center;
        gap: 8px;
      }
    }

    :deep(.wf-menu-soon) {
      transform: scale(0.85);
      padding: 0 6px;
      height: 18px;
      line-height: 18px;
    }

    :deep(.el-menu-item:hover),
    :deep(.el-sub-menu__title:hover) {
      background: var(--wf-tab-active-bg);
      color: var(--wf-tab-primary);
    }
  }
}
</style>
