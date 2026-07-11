<script setup lang="ts">
import { computed } from 'vue'
import { useAppStore } from '@/stores/layout/app'
import { useTabsStore } from '@/stores/layout/tabs'

const tabsStore = useTabsStore()
const appStore = useAppStore()

const viewKey = computed(() => tabsStore.refreshKeys[tabsStore.activePath] || 0)
</script>

<template>
  <div v-loading="appStore.globalLoading" class="wf-content">
    <el-scrollbar class="wf-content__scroll">
      <router-view v-slot="{ Component, route }">
        <keep-alive :include="tabsStore.cachedRouteNames">
          <component
            :is="Component"
            v-if="Component"
            :key="`${route.name as string}-${viewKey}`"
          />
        </keep-alive>
      </router-view>
    </el-scrollbar>
  </div>
</template>

<style scoped lang="scss">
.wf-content {
  flex: 1;
  min-height: 0;
  background: var(--wf-content-bg);
  border-left: 1px solid var(--wf-border);
  box-sizing: border-box;

  &__scroll {
    height: 100%;

    :deep(.el-scrollbar__view) {
      min-height: 100%;
      padding: 0;
      box-sizing: border-box;
    }
  }
}
</style>
