<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { ArrowDown } from '@element-plus/icons-vue'
import { localeOptions, type AppLocale } from '@/i18n/config'
import { setAppLocale } from '@/i18n'

const { locale, t } = useI18n()

const currentLabel = computed(() => {
  const option = localeOptions.find((item) => item.value === locale.value)
  return option ? t(option.labelKey) : t('locale.zhCN')
})

function handleCommand(value: AppLocale) {
  setAppLocale(value)
}
</script>

<template>
  <el-dropdown trigger="click" @command="handleCommand">
    <div class="wf-locale-switch">
      <span>{{ currentLabel }}</span>
      <el-icon><ArrowDown /></el-icon>
    </div>
    <template #dropdown>
      <el-dropdown-menu>
        <el-dropdown-item
          v-for="item in localeOptions"
          :key="item.value"
          :command="item.value"
          :class="{ active: locale === item.value }"
        >
          {{ t(item.labelKey) }}
        </el-dropdown-item>
      </el-dropdown-menu>
    </template>
  </el-dropdown>
</template>

<style scoped lang="scss">
.wf-locale-switch {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  cursor: pointer;
  user-select: none;
  color: #606266;
  font-size: 14px;
}

:deep(.active) {
  color: #1890ff;
  font-weight: 600;
}
</style>
