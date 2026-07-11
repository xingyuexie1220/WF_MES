<script setup lang="ts">
defineOptions({ name: 'BigScreen' })
import { onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { useUserStore } from '@/stores/auth/user'
import { useDashboardStore } from '@/stores/dashboard/dashboard'

const { t } = useI18n()
const userStore = useUserStore()
const dashboardStore = useDashboardStore()

onMounted(() => {
  dashboardStore.fetchBigScreenOverview()
})
</script>

<template>
  <div class="bigscreen">
    <header class="bigscreen__header">
      <h1>{{ t('menu.bigscreen') }}</h1>
      <div class="bigscreen__meta">
        <span>{{ userStore.userInfo?.factoryName }}</span>
        <span>{{ dashboardStore.bigScreenOverview?.status || t('layout.developing') }}</span>
      </div>
    </header>
    <div class="bigscreen__grid">
      <div v-for="n in 4" :key="n" class="bigscreen__card">
        <div class="bigscreen__card-title">KPI {{ n }}</div>
        <div class="bigscreen__card-value">--</div>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.bigscreen {
  min-height: 100vh;
  background: #0b1220;
  color: #e5eaf3;
  padding: 24px;

  &__header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 24px;
  }

  &__meta {
    display: flex;
    gap: 16px;
    opacity: 0.85;
  }

  &__grid {
    display: grid;
    grid-template-columns: repeat(4, 1fr);
    gap: 16px;
  }

  &__card {
    background: rgba(255, 255, 255, 0.06);
    border: 1px solid rgba(255, 255, 255, 0.08);
    border-radius: 12px;
    padding: 24px;
    min-height: 140px;
  }

  &__card-title {
    font-size: 14px;
    opacity: 0.75;
  }

  &__card-value {
    margin-top: 12px;
    font-size: 36px;
    font-weight: 700;
  }
}
</style>
