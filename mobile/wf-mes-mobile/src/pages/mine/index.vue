<script setup lang="ts">
import { computed } from 'vue'
import { onShow } from '@dcloudio/uni-app'
import { useI18n } from 'vue-i18n'
import { appConfig } from '@/config/app'
import WfListRow from '@/components/WfListRow.vue'
import WfTabBar from '@/components/WfTabBar.vue'
import { useUserStore } from '@/stores/user'
import { useLocaleStore } from '@/stores/locale'
import type { AppLocale } from '@/i18n'

const { t } = useI18n()
const userStore = useUserStore()
const localeStore = useLocaleStore()

const avatarText = computed(() => (userStore.displayName || '?').slice(0, 1).toUpperCase())
const localeLabel = computed(() => t(localeStore.getLocaleLabel(localeStore.locale)))

onShow(() => {
  uni.setNavigationBarTitle({ title: t('mobile.tab.mine') })
  if (!userStore.checkAuthGuard()) return
})

async function refreshProfile() {
  await userStore.fetchUserInfo()
  uni.showToast({ title: t('mobile.mine.updated'), icon: 'success' })
}

function openLocalePicker() {
  uni.showActionSheet({
    itemList: localeStore.localeOptions.map((item) => t(item.labelKey)),
    success(res) {
      const next = localeStore.localeOptions[res.tapIndex]?.value as AppLocale | undefined
      if (next) {
        localeStore.applyLocale(next)
      }
    }
  })
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
    <view class="profile">
      <view class="profile__avatar">{{ avatarText }}</view>
      <view class="profile__info">
        <text class="profile__name">{{ userStore.displayName }}</text>
        <text class="profile__account">@{{ userStore.userInfo?.userName }}</text>
        <text v-if="userStore.currentFactory.name" class="profile__factory">
          {{ userStore.currentFactory.name }}
        </text>
      </view>
    </view>

    <view class="list-card">
      <WfListRow
        :title="t('mobile.mine.refreshProfile')"
        icon-name="reload"
        icon-bg="#dbeafe"
        icon-fg="#2563eb"
        border
        @click="refreshProfile"
      />
      <WfListRow
        :title="t('locale.title')"
        :value="localeLabel"
        icon-name="zh"
        icon-bg="#ede9fe"
        icon-fg="#7c3aed"
        border
        @click="openLocalePicker"
      />
      <WfListRow
        :title="t('mobile.mine.changePassword')"
        icon-name="lock"
        icon-bg="#ffedd5"
        icon-fg="#ea580c"
        border
        @click="goChangePassword"
      />
      <WfListRow
        :title="t('mobile.mine.about')"
        :value="appConfig.appName"
        icon-name="info-circle"
        icon-bg="#e0e7ff"
        icon-fg="#4f46e5"
        :show-arrow="false"
        border
      />
      <WfListRow
        :title="t('mobile.mine.version')"
        value="0.1.0"
        icon-name="integral"
        icon-bg="#f1f5f9"
        icon-fg="#64748b"
        :show-arrow="false"
      />
    </view>

    <button class="btn-logout" @click="handleLogout">{{ t('mobile.mine.logout') }}</button>

    <WfTabBar active="mine" />
  </view>
</template>

<style scoped lang="scss">
@import '@/styles/tokens.scss';

.page {
  min-height: 100vh;
  background: $wf-bg;
  padding: $wf-page-pad-y $wf-page-pad-x 48rpx;
  box-sizing: border-box;
}

.profile {
  display: flex;
  align-items: center;
  gap: 24rpx;
  padding: 28rpx;
  margin-bottom: $wf-section-gap;
  background: linear-gradient(160deg, #eff6ff 0%, #ffffff 70%);
  border-radius: $wf-radius-lg;
  border: 1rpx solid $wf-border;
  box-shadow: $wf-shadow;
}

.profile__avatar {
  width: 96rpx;
  height: 96rpx;
  border-radius: 50%;
  background: linear-gradient(135deg, $wf-primary, $wf-primary-dark);
  color: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 40rpx;
  font-weight: 700;
  flex-shrink: 0;
}

.profile__name {
  display: block;
  font-size: 34rpx;
  font-weight: 700;
  color: $wf-text;
}

.profile__account,
.profile__factory {
  display: block;
  margin-top: 6rpx;
  font-size: 24rpx;
  color: $wf-text-secondary;
}

.list-card {
  background: $wf-card;
  border-radius: $wf-radius-md;
  overflow: hidden;
  margin-bottom: 32rpx;
  box-shadow: $wf-shadow;
}

.btn-logout {
  height: 88rpx;
  line-height: 88rpx;
  background: $wf-card;
  color: $wf-danger;
  border-radius: $wf-radius-sm;
  font-size: 30rpx;
  font-weight: 500;
  border: none;
  box-shadow: $wf-shadow;
}

.btn-logout::after {
  border: none;
}
</style>
