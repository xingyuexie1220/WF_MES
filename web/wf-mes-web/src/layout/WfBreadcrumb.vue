<script setup lang="ts">
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useTabsStore } from '@/stores/layout/tabs'

const route = useRoute()
const router = useRouter()
const tabsStore = useTabsStore()
const { t } = useI18n()

const items = computed(() => {
  const crumbs = [{ label: t('layout.home'), path: '/dashboard' }]
  if (route.path === '/dashboard') {
    return crumbs
  }
  const tab = tabsStore.tabs.find((item) => item.path === route.path)
  if (tab) {
    crumbs.push({ label: tabsStore.getTabLabel(tab, t), path: route.path })
  }
  return crumbs
})

function navigate(path: string) {
  if (route.path !== path) {
    router.push(path)
  }
}
</script>

<template>
  <el-breadcrumb class="wf-breadcrumb" separator="/">
    <el-breadcrumb-item v-for="item in items" :key="item.path">
      <span
        class="wf-breadcrumb__item"
        :class="{ 'is-link': item.path !== route.path }"
        @click="item.path !== route.path && navigate(item.path)"
      >
        {{ item.label }}
      </span>
    </el-breadcrumb-item>
  </el-breadcrumb>
</template>

<style scoped lang="scss">
.wf-breadcrumb {
  :deep(.el-breadcrumb__inner),
  :deep(.el-breadcrumb__separator) {
    color: rgba(255, 255, 255, 0.88);
    font-weight: 400;
  }

  &__item {
    color: rgba(255, 255, 255, 0.88);

    &.is-link {
      cursor: pointer;

      &:hover {
        color: #fff;
      }
    }
  }
}
</style>
