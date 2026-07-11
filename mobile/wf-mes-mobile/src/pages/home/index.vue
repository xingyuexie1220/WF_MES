<script setup lang="ts">
import { computed, ref } from 'vue'
import { onShow } from '@dcloudio/uni-app'
import { useI18n } from 'vue-i18n'
import { useUserStore } from '@/stores/user'
import { t as translate } from '@/i18n'

interface MenuCard {
  title: string
  path?: string
  icon?: string
  desc?: string
}

const { t, locale } = useI18n()
const userStore = useUserStore()
const cards = ref<MenuCard[]>([])

const today = computed(() =>
  new Date().toLocaleDateString(locale.value, { month: 'long', day: 'numeric', weekday: 'long' })
)

const quickActions = computed<MenuCard[]>(() => [
  {
    title: t('menu.mobile.warehouseScan'),
    desc: t('mobile.scan.warehouseDesc'),
    path: '/pages/warehouse/scan/index',
    icon: '📦'
  },
  {
    title: t('menu.mobile.simplePass'),
    desc: t('mobile.scan.simplePassDesc'),
    path: '/pages/mes/simple-pass/index',
    icon: '✅'
  },
  {
    title: t('menu.mobile.inventory'),
    desc: t('layout.developing'),
    path: '/pages/inventory/list/index',
    icon: '📋'
  }
])

onShow(async () => {
  uni.setNavigationBarTitle({ title: t('mobile.tab.home') })
  if (!userStore.checkAuthGuard()) return

  try {
    const menus = await userStore.fetchMobileMenus()
    cards.value = flattenMenus(menus)
  } catch {
    cards.value = []
  }
})

function toPageUrl(path: string) {
  const normalized = path.startsWith('/') ? path : `/${path}`
  return normalized.endsWith('/index') ? normalized : `${normalized}/index`
}

function flattenMenus(menus: Array<{ title?: string; i18nKey?: string; path?: string; icon?: string; children?: unknown[] }>): MenuCard[] {
  const result: MenuCard[] = []
  for (const menu of menus) {
    if (menu.path?.includes('/pages/')) {
      result.push({
        title: menu.i18nKey ? translate(menu.i18nKey) : (menu.title || ''),
        path: toPageUrl(menu.path),
        icon: menu.icon || '📱'
      })
    }
    if (Array.isArray(menu.children)) {
      result.push(...flattenMenus(menu.children as typeof menus))
    }
  }
  return result
}

function openPage(path?: string) {
  if (!path) return
  if (path.includes('/pages/home/') || path.includes('/pages/scan/') || path.includes('/pages/mine/')) {
    uni.switchTab({ url: path })
    return
  }
  uni.navigateTo({ url: path })
}
</script>

<template>
  <view class="page">
    <view class="header">
      <view>
        <text class="greeting">{{ t('login.welcome') }}，{{ userStore.displayName }}</text>
        <text class="date">{{ today }}</text>
        <text v-if="userStore.currentFactory.name" class="factory">{{ userStore.currentFactory.name }}</text>
      </view>
      <view class="badge">{{ t('mobile.home.badge') }}</view>
    </view>

    <view class="section-title">{{ t('mobile.home.quickActions') }}</view>
    <view class="actions">
      <view v-for="action in quickActions" :key="action.path" class="action-item" @click="openPage(action.path)">
        <text class="action-icon">{{ action.icon }}</text>
        <text class="action-title">{{ action.title }}</text>
        <text class="action-desc">{{ action.desc }}</text>
      </view>
    </view>

    <view class="section-title">{{ t('mobile.home.menuTitle') }}</view>
    <view v-if="cards.length === 0" class="empty">{{ t('common.noData') }}</view>
    <view class="cards">
      <view v-for="card in cards" :key="card.path" class="card" @click="openPage(card.path)">
        <text class="card-icon">{{ card.icon || '📱' }}</text>
        <text class="card-title">{{ card.title }}</text>
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

.header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 28rpx;
}

.greeting {
  display: block;
  font-size: 40rpx;
  font-weight: 700;
  color: #0f172a;
}

.date,
.factory {
  display: block;
  margin-top: 8rpx;
  font-size: 24rpx;
  color: #64748b;
}

.badge {
  padding: 8rpx 20rpx;
  background: #dbeafe;
  color: #2563eb;
  border-radius: 999rpx;
  font-size: 22rpx;
}

.section-title {
  font-size: 30rpx;
  font-weight: 600;
  color: #0f172a;
  margin-bottom: 20rpx;
}

.actions {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16rpx;
  margin-bottom: 32rpx;
}

.action-item,
.card {
  background: #fff;
  border-radius: 20rpx;
  box-shadow: 0 4rpx 16rpx rgba(15, 35, 80, 0.05);
}

.action-item {
  padding: 28rpx 24rpx;
}

.action-icon,
.card-icon {
  font-size: 44rpx;
}

.action-title,
.card-title {
  display: block;
  margin-top: 12rpx;
  font-size: 28rpx;
  font-weight: 600;
  color: #0f172a;
}

.action-desc {
  display: block;
  margin-top: 6rpx;
  font-size: 22rpx;
  color: #94a3b8;
}

.cards {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16rpx;
}

.card {
  padding: 28rpx 24rpx;
}

.empty {
  color: #94a3b8;
  margin-bottom: 24rpx;
}
</style>
