<script setup lang="ts">
import { onShow } from '@dcloudio/uni-app'
import { useI18n } from 'vue-i18n'
import { useUserStore } from '@/stores/user'

const { t } = useI18n()
const userStore = useUserStore()

const entries = [
  { titleKey: 'menu.mobile.warehouseScan', descKey: 'mobile.scan.warehouseDesc', path: '/pages/warehouse/scan/index', icon: '📦' },
  { titleKey: 'menu.mobile.simplePass', descKey: 'mobile.scan.simplePassDesc', path: '/pages/mes/simple-pass/index', icon: '✅' }
]

onShow(() => {
  uni.setNavigationBarTitle({ title: t('mobile.tab.scan') })
  userStore.checkAuthGuard()
})

function openPage(path: string) {
  uni.navigateTo({ url: path })
}

function quickScan() {
  uni.scanCode({
    success(res) {
      uni.showModal({
        title: t('mobile.scan.hubTitle'),
        content: res.result,
        confirmText: t('common.confirm')
      })
    }
  })
}
</script>

<template>
  <view class="page">
    <view class="hero-card">
      <text class="title">{{ t('mobile.scan.hubTitle') }}</text>
      <text class="hint">{{ t('mobile.scan.hint') }}</text>
      <button class="scan-btn" @click="quickScan">{{ t('mobile.scan.startScan') }}</button>
    </view>

    <view class="section-title">{{ t('mobile.home.quickActions') }}</view>
    <view class="entries">
      <view v-for="item in entries" :key="item.path" class="entry" @click="openPage(item.path)">
        <text class="icon">{{ item.icon }}</text>
        <view class="content">
          <text class="entry-title">{{ t(item.titleKey) }}</text>
          <text class="entry-desc">{{ t(item.descKey) }}</text>
        </view>
        <text class="arrow">›</text>
      </view>
    </view>
  </view>
</template>

<style scoped lang="scss">
.page {
  min-height: 100vh;
  background: #f1f5f9;
  padding: 32rpx 28rpx;
  box-sizing: border-box;
}

.hero-card {
  background: linear-gradient(135deg, #2563eb, #1d4ed8);
  border-radius: 24rpx;
  padding: 40rpx 32rpx;
  color: #fff;
  margin-bottom: 32rpx;
}

.title {
  display: block;
  font-size: 36rpx;
  font-weight: 700;
}

.hint {
  display: block;
  margin-top: 12rpx;
  font-size: 24rpx;
  opacity: 0.85;
}

.scan-btn {
  margin-top: 28rpx;
  background: #fff;
  color: #2563eb;
  border-radius: 16rpx;
  font-size: 30rpx;
  border: none;
}

.section-title {
  font-size: 30rpx;
  font-weight: 600;
  color: #0f172a;
  margin-bottom: 20rpx;
}

.entries {
  display: flex;
  flex-direction: column;
  gap: 16rpx;
}

.entry {
  display: flex;
  align-items: center;
  gap: 20rpx;
  background: #fff;
  border-radius: 20rpx;
  padding: 28rpx 24rpx;
}

.icon {
  font-size: 44rpx;
}

.content {
  flex: 1;
}

.entry-title {
  display: block;
  font-size: 28rpx;
  font-weight: 600;
  color: #0f172a;
}

.entry-desc {
  display: block;
  margin-top: 6rpx;
  font-size: 22rpx;
  color: #94a3b8;
}

.arrow {
  color: #cbd5e1;
  font-size: 36rpx;
}
</style>
