<script setup lang="ts">
import { ref } from 'vue'
import { onShow } from '@dcloudio/uni-app'
import { useI18n } from 'vue-i18n'
import { request } from '@/utils/request'
import { useUserStore } from '@/stores/user'

const { t } = useI18n()
const userStore = useUserStore()
const barcode = ref('')
const records = ref<Array<Record<string, unknown>>>([])

onShow(() => {
  uni.setNavigationBarTitle({ title: t('menu.mobile.simplePass') })
  userStore.checkAuthGuard()
})

async function handlePass() {
  if (!barcode.value.trim()) {
    uni.showToast({ title: t('mobile.pass.scanFirst'), icon: 'none' })
    return
  }
  const data = await request<Record<string, unknown>>('/production/pass-records', 'POST', { barcode: barcode.value })
  records.value.unshift(data)
  uni.showToast({ title: t('common.success'), icon: 'success' })
}

function handleScan() {
  uni.scanCode({
    success: (res) => {
      barcode.value = res.result
    }
  })
}
</script>

<template>
  <view class="page">
    <view class="scan">{{ barcode || t('mobile.scan.hint') }}</view>
    <view class="actions">
      <button @click="handleScan">{{ t('mobile.pass.scanBtn') }}</button>
      <button type="primary" @click="handlePass">{{ t('mobile.pass.submitBtn') }}</button>
    </view>
    <view v-for="(item, index) in records" :key="index" class="card">{{ JSON.stringify(item) }}</view>
  </view>
</template>

<style scoped lang="scss">
.page { padding: 32rpx; }
.scan { background: #fff; padding: 32rpx; border-radius: 12rpx; margin-bottom: 24rpx; min-height: 120rpx; }
.actions { display: flex; gap: 16rpx; margin-bottom: 24rpx; }
.card { background: #fff; padding: 24rpx; border-radius: 12rpx; margin-bottom: 16rpx; font-size: 24rpx; }
</style>
