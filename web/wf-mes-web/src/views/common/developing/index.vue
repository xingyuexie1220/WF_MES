<template>
  <el-card shadow="never" class="wf-developing">
    <el-empty :description="t('layout.developing')">
      <template #image>
        <div class="wf-developing__icon">🚧</div>
      </template>
      <p v-if="pageTitle" class="wf-developing__title">{{ pageTitle }}</p>
      <p class="wf-developing__hint">{{ t('common.placeholder') }}</p>
    </el-empty>
  </el-card>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { useI18n } from 'vue-i18n'

const route = useRoute()
const { t } = useI18n()

const pageTitle = computed(() => {
  const key = route.meta.titleKey
  return typeof key === 'string' ? t(key) : ''
})
</script>

<style scoped lang="scss">
.wf-developing {
  min-height: 360px;
  display: flex;
  align-items: center;
  justify-content: center;

  &__icon {
    font-size: 48px;
    line-height: 1;
    margin-bottom: 8px;
  }

  &__title {
    margin: 0 0 8px;
    font-size: 18px;
    font-weight: 600;
    color: var(--wf-text);
  }

  &__hint {
    margin: 0;
    font-size: 13px;
    color: var(--wf-text-secondary);
  }
}
</style>
