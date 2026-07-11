<script setup lang="ts">
import { ref } from 'vue'
import { onShow } from '@dcloudio/uni-app'
import { useI18n } from 'vue-i18n'
import { request } from '@/utils/request'
import { useUserStore } from '@/stores/user'

const { t } = useI18n()
const userStore = useUserStore()
const scanCode = ref('')
const results = ref<Array<{ barcode: string; status: string }>>([])

onShow(() => {
  uni.setNavigationBarTitle({ title: t('menu.mobile.warehouseScan') })
  userStore.checkAuthGuard()
})

async function handleScan() {
  uni.scanCode({
    success: async (res) => {
      scanCode.value = res.result
      const data = await request<{ barcode: string; status: string }>('/warehouse/inbound/scan', 'POST', { barcode: res.result })
      results.value.unshift(data)
      uni.showToast({ title: t('common.success'), icon: 'success' })
    }
  })
}
</script>

<template>
  <view class="page">
    <view class="scan-box">{{ scanCode || t('mobile.scan.hint') }}</view>
    <button type="primary" @click="handleScan">{{ t('mobile.scan.startScan') }}</button>
    <view v-for="item in results" :key="item.barcode" class="result">{{ item.barcode }} - {{ item.status }}</view>
  </view>
</template>

<style scoped lang="scss">
.page { padding: 32rpx; }
.scan-box { background: #fff; padding: 32rpx; border-radius: 12rpx; margin-bottom: 24rpx; min-height: 120rpx; }
.result { margin-top: 16rpx; padding: 16rpx; background: #fff; border-radius: 8rpx; }
</style>
