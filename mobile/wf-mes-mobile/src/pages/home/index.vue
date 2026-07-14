<script setup lang="ts">
import { ref } from 'vue'
import { onShow } from '@dcloudio/uni-app'
import { useI18n } from 'vue-i18n'
import WfMenuIcon from '@/components/WfMenuIcon.vue'
import WfTabBar from '@/components/WfTabBar.vue'
import { useUserStore } from '@/stores/user'
import { t as translate } from '@/i18n'
import { isDevelopingPath, isTabMenuPath } from '@/utils/menuIcon'

interface MenuCard {
  title: string
  path: string
  icon?: string
  developing?: boolean
}

const { t } = useI18n()
const userStore = useUserStore()
const cards = ref<MenuCard[]>([])

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

function flattenMenus(
  menus: Array<{ title?: string; i18nKey?: string; path?: string; icon?: string; children?: unknown[] }>
): MenuCard[] {
  const result: MenuCard[] = []
  for (const menu of menus) {
    if (menu.path?.includes('/pages/') && !isTabMenuPath(menu.path)) {
      const path = toPageUrl(menu.path)
      result.push({
        title: menu.i18nKey ? translate(menu.i18nKey) : menu.title || '',
        path,
        icon: menu.icon,
        developing: isDevelopingPath(path)
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
  if (isTabMenuPath(path)) {
    uni.switchTab({ url: path })
    return
  }
  uni.navigateTo({ url: path })
}
</script>

<template>
  <view class="page">
    <view class="header">
      <view class="header__text">
        <text class="header__hello">{{ t('mobile.home.hello') }}，{{ userStore.displayName }}</text>
        <text v-if="userStore.currentFactory.name" class="header__factory">
          {{ userStore.currentFactory.name }}
        </text>
      </view>
      <view class="header__avatar">{{ (userStore.displayName || '?').slice(0, 1) }}</view>
    </view>

    <view class="section">
      <text class="section__title">{{ t('mobile.home.menuTitle') }}</text>
      <view v-if="cards.length" class="grid-card">
        <view
          v-for="card in cards"
          :key="card.path"
          class="grid-item"
          :class="{ 'grid-item--muted': card.developing }"
          @click="openPage(card.path)"
        >
          <WfMenuIcon :icon="card.icon" :path="card.path" />
          <text class="grid-item__title">{{ card.title }}</text>
          <text v-if="card.developing" class="grid-item__tag">{{ t('layout.developing') }}</text>
        </view>
      </view>
      <view v-else class="empty">{{ t('common.noData') }}</view>
    </view>

    <WfTabBar active="home" />
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

.header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 20rpx;
  margin-bottom: $wf-section-gap;
  padding: 8rpx 4rpx 4rpx;
}

.header__hello {
  display: block;
  font-size: 36rpx;
  font-weight: 700;
  color: $wf-text;
  line-height: 1.3;
}

.header__factory {
  display: block;
  margin-top: 8rpx;
  font-size: 24rpx;
  color: $wf-text-secondary;
}

.header__avatar {
  width: 72rpx;
  height: 72rpx;
  border-radius: 50%;
  background: linear-gradient(135deg, $wf-primary, $wf-primary-dark);
  color: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 28rpx;
  font-weight: 700;
  flex-shrink: 0;
}

.section__title {
  display: block;
  margin-bottom: 16rpx;
  padding-left: 4rpx;
  font-size: 28rpx;
  font-weight: 600;
  color: #334155;
}

.grid-card {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 24rpx 8rpx;
  padding: 32rpx 16rpx 28rpx;
  background: $wf-card;
  border-radius: $wf-radius-md;
  box-shadow: $wf-shadow;
}

.grid-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12rpx;
  min-height: 132rpx;
  padding: 4rpx;
}

.grid-item--muted {
  opacity: 0.7;
}

.grid-item:active {
  opacity: 0.75;
}

.grid-item__title {
  font-size: 22rpx;
  color: #334155;
  text-align: center;
  line-height: 1.35;
}

.grid-item__tag {
  font-size: 18rpx;
  color: $wf-text-muted;
  line-height: 1.2;
}

.empty {
  padding: 56rpx 24rpx;
  text-align: center;
  color: $wf-text-muted;
  font-size: 26rpx;
  background: $wf-card;
  border-radius: $wf-radius-md;
  box-shadow: $wf-shadow;
}
</style>
