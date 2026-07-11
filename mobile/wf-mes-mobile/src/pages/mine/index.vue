<script setup lang="ts">
import { computed, ref } from 'vue'
import { onShow } from '@dcloudio/uni-app'
import { useI18n } from 'vue-i18n'
import { appConfig } from '@/config/app'
import WfLocalePicker from '@/components/WfLocalePicker.vue'
import WfFactoryPicker from '@/components/WfFactoryPicker.vue'
import { useUserStore } from '@/stores/user'
import type { FactorySummary } from '@/types/auth'

const { t } = useI18n()
const userStore = useUserStore()
const showFactoryPicker = ref(false)

const avatarText = computed(() => (userStore.displayName || '?').slice(0, 1).toUpperCase())
const factories = computed(() => userStore.accessibleFactories)

onShow(() => {
  uni.setNavigationBarTitle({ title: t('mobile.tab.mine') })
  if (!userStore.checkAuthGuard()) return
})

async function refreshProfile() {
  await userStore.fetchUserInfo()
  uni.showToast({ title: t('mobile.mine.updated'), icon: 'success' })
}

function openFactoryPicker() {
  if (factories.value.length <= 1) {
    uni.showToast({ title: t('common.noData'), icon: 'none' })
    return
  }
  showFactoryPicker.value = true
}

async function handleSwitchFactory(factory: FactorySummary) {
  if (factory.id === userStore.currentFactory.id) {
    showFactoryPicker.value = false
    return
  }
  await userStore.switchFactory(factory.id)
  showFactoryPicker.value = false
  uni.showToast({ title: t('mobile.factory.switchSuccess'), icon: 'success' })
}

function goChangePassword() {
  uni.navigateTo({ url: '/pages/change-password/index' })
}

function handleLogout() {
  uni.showModal({
    title: t('mobile.mine.logoutTitle'),
    content: t('mobile.mine.logoutConfirm'),
    confirmText: t('common.confirm'),
    cancelText: t('common.cancel'),
    success(res) {
      if (res.confirm) {
        userStore.logout()
      }
    }
  })
}
</script>

<template>
  <view class="page">
    <view class="profile-card">
      <view class="avatar">{{ avatarText }}</view>
      <view class="info">
        <text class="name">{{ userStore.displayName }}</text>
        <text class="account">@{{ userStore.userInfo?.userName }}</text>
        <text v-if="userStore.currentFactory.name" class="factory">{{ userStore.currentFactory.name }}</text>
      </view>
    </view>

    <view class="menu">
      <view class="menu-item" @click="refreshProfile">
        <text>{{ t('mobile.mine.refreshProfile') }}</text>
        <text class="arrow">›</text>
      </view>
      <view class="menu-item" @click="openFactoryPicker">
        <text>{{ t('mobile.factory.switch') }}</text>
        <text class="value">{{ userStore.currentFactory.name || '-' }}</text>
      </view>
      <view class="menu-item">
        <WfLocalePicker />
      </view>
      <view class="menu-item" @click="goChangePassword">
        <text>{{ t('mobile.mine.changePassword') }}</text>
        <text class="arrow">›</text>
      </view>
      <view class="menu-item">
        <text>{{ t('mobile.mine.about') }}</text>
        <text class="value">{{ appConfig.appName }}</text>
      </view>
      <view class="menu-item">
        <text>{{ t('mobile.mine.version') }}</text>
        <text class="value">0.1.0</text>
      </view>
    </view>

    <button class="btn-logout" @click="handleLogout">{{ t('mobile.mine.logout') }}</button>

    <WfFactoryPicker
      :show="showFactoryPicker"
      :factories="factories"
      :current-factory-id="userStore.currentFactory.id"
      @close="showFactoryPicker = false"
      @select="handleSwitchFactory"
    />
  </view>
</template>

<style scoped lang="scss">
.page {
  min-height: 100vh;
  background: #f1f5f9;
  padding: 32rpx 28rpx;
  box-sizing: border-box;
}

.profile-card {
  display: flex;
  align-items: center;
  gap: 24rpx;
  background: #fff;
  border-radius: 24rpx;
  padding: 36rpx;
  margin-bottom: 24rpx;
}

.avatar {
  width: 120rpx;
  height: 120rpx;
  border-radius: 50%;
  background: linear-gradient(135deg, #2563eb, #1d4ed8);
  color: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 48rpx;
  font-weight: 700;
}

.name {
  display: block;
  font-size: 36rpx;
  font-weight: 700;
  color: #0f172a;
}

.account,
.factory {
  display: block;
  margin-top: 8rpx;
  font-size: 24rpx;
  color: #64748b;
}

.menu {
  background: #fff;
  border-radius: 20rpx;
  overflow: hidden;
  margin-bottom: 32rpx;
}

.menu-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 32rpx 28rpx;
  font-size: 30rpx;
  color: #0f172a;
  border-bottom: 1rpx solid #f1f5f9;
}

.menu-item:last-child {
  border-bottom: none;
}

.arrow {
  color: #cbd5e1;
  font-size: 36rpx;
}

.value {
  font-size: 24rpx;
  color: #64748b;
}

.btn-logout {
  background: #fff;
  color: #ef4444;
  border-radius: 16rpx;
  font-size: 30rpx;
  border: none;
}
</style>
