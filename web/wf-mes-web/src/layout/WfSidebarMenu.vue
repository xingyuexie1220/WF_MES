<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import WfSidebarMenu from './WfSidebarMenu.vue'
import { resolveMenuLabel, resolveMenuIcon, type SidebarMenuItem } from '@/utils/menu'

defineOptions({ name: 'WfSidebarMenu' })

defineProps<{
  items: SidebarMenuItem[]
}>()

const { t } = useI18n()

function getMenuLabel(item: SidebarMenuItem) {
  return resolveMenuLabel(item, t)
}
</script>

<template>
  <template v-for="item in items" :key="item.id">
    <el-sub-menu v-if="item.children?.length" :index="item.index">
      <template #title>
        <el-icon><component :is="resolveMenuIcon(item.icon)" /></el-icon>
        <span>{{ getMenuLabel(item) }}</span>
      </template>
      <WfSidebarMenu :items="item.children" />
    </el-sub-menu>
    <el-menu-item
      v-else
      :index="item.path || item.index"
      :disabled="item.placeholder"
      :class="{ 'wf-menu-item--placeholder': item.placeholder }"
    >
      <el-icon v-if="item.icon"><component :is="resolveMenuIcon(item.icon)" /></el-icon>
      <template #title>
        <span>{{ getMenuLabel(item) }}</span>
        <el-tag v-if="item.placeholder" size="small" type="info" class="wf-menu-soon">{{ t('layout.comingSoon') }}</el-tag>
      </template>
    </el-menu-item>
  </template>
</template>
