<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { ElMessage } from 'element-plus'
import { useUserStore } from '@/stores/auth/user'
import { useMenuStore } from '@/stores/layout/menu'
import type { FactorySummary } from '@/types/common/factory'

const userStore = useUserStore()
const menuStore = useMenuStore()
const { t } = useI18n()

const currentFactoryId = computed(() => userStore.userInfo?.factoryId ?? (Number(localStorage.getItem('wf_factory_id') || 0) || undefined))

const factories = computed(() => userStore.userInfo?.accessibleFactories ?? [])

const currentLabel = computed(() => {
  const current = factories.value.find((f) => f.id === currentFactoryId.value)
  return current?.factoryName || userStore.userInfo?.factoryName || t('factory.select')
})

async function handleSwitch(factory: FactorySummary) {
  if (factory.id === currentFactoryId.value) {
    return
  }
  try {
    await userStore.switchFactory(factory.id)
    await userStore.fetchUserInfo()
    await menuStore.loadMenus(true)
    ElMessage.success(t('factory.switchSuccess'))
  } catch {
    /* handled by request interceptor */
  }
}
</script>

<template>
  <el-dropdown v-if="factories.length" trigger="click" class="wf-factory-switch" @command="handleSwitch">
    <span class="wf-factory-switch__trigger">
      {{ currentLabel }}
      <span class="wf-factory-switch__arrow">▾</span>
    </span>
    <template #dropdown>
      <el-dropdown-menu>
        <el-dropdown-item
          v-for="item in factories"
          :key="item.id"
          :command="item"
          :class="{ 'is-active': item.id === currentFactoryId }"
        >
          {{ item.factoryName }}
          <span v-if="item.isDefault" class="wf-factory-switch__default">{{ t('factory.default') }}</span>
        </el-dropdown-item>
      </el-dropdown-menu>
    </template>
  </el-dropdown>
</template>

<style scoped lang="scss">
.wf-factory-switch {
  &__trigger {
    display: inline-flex;
    align-items: center;
    gap: 4px;
    color: #fff;
    font-size: 13px;
    cursor: pointer;
    padding: 6px 10px;
    border-radius: 4px;

    &:hover {
      background: rgba(255, 255, 255, 0.12);
    }
  }

  &__arrow {
    font-size: 10px;
    opacity: 0.85;
  }

  &__default {
    margin-left: 6px;
    font-size: 11px;
    color: #909399;
  }
}

:deep(.el-dropdown-menu__item.is-active) {
  color: var(--el-color-primary);
  font-weight: 600;
}
</style>
