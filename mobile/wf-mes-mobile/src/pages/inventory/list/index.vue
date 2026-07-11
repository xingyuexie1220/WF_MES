<script setup lang="ts">
import { onShow } from '@dcloudio/uni-app'
import { ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { request } from '@/utils/request'
import { useUserStore } from '@/stores/user'

const { t } = useI18n()
const userStore = useUserStore()
const items = ref<Array<Record<string, unknown>>>([])

onShow(async () => {
  uni.setNavigationBarTitle({ title: t('menu.mobile.inventory') })
  if (!userStore.checkAuthGuard()) return
  const data = await request<{ items?: Array<Record<string, unknown>> }>('/warehouse/inbound', 'GET')
  items.value = data.items ?? []
})
</script>

<template>
  <view class="page">
    <view v-if="items.length === 0" class="empty">{{ t('common.noData') }}</view>
    <view v-for="(item, index) in items" :key="index" class="card">{{ JSON.stringify(item) }}</view>
  </view>
</template>

<style scoped lang="scss">
.page { padding: 32rpx; }
.empty { color: #888; }
.card { background: #fff; padding: 24rpx; border-radius: 12rpx; margin-bottom: 16rpx; }
</style>
