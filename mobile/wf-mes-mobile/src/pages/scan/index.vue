<script setup lang="ts">
import { ref } from 'vue'
import { onShow } from '@dcloudio/uni-app'
import { useI18n } from 'vue-i18n'
import WfListRow from '@/components/WfListRow.vue'
import WfTabBar from '@/components/WfTabBar.vue'
import { useUserStore } from '@/stores/user'
import { t as translate } from '@/i18n'
import { isScanBizPath, isTabMenuPath } from '@/utils/menuIcon'

interface ScanEntry {
  title: string
  desc: string
  path: string
  icon?: string
}

const { t } = useI18n()
const userStore = useUserStore()
const entries = ref<ScanEntry[]>([])

const descKeyMap: Record<string, string> = {
  'warehouse/scan': 'mobile.scan.warehouseDesc',
  'simple-pass': 'mobile.scan.simplePassDesc',
  'mes/report': 'mobile.scan.reportDesc'
}

function toPageUrl(path: string) {
  const normalized = path.startsWith('/') ? path : `/${path}`
  return normalized.endsWith('/index') ? normalized : `${normalized}/index`
}

function resolveDesc(path: string) {
  const key = Object.keys(descKeyMap).find((k) => path.includes(k))
  return key ? t(descKeyMap[key]) : ''
}

function flattenScanMenus(
  menus: Array<{ title?: string; i18nKey?: string; path?: string; icon?: string; children?: unknown[] }>
): ScanEntry[] {
  const result: ScanEntry[] = []
  for (const menu of menus) {
    if (menu.path?.includes('/pages/') && !isTabMenuPath(menu.path) && isScanBizPath(menu.path)) {
      const path = toPageUrl(menu.path)
      result.push({
        title: menu.i18nKey ? translate(menu.i18nKey) : menu.title || '',
        desc: resolveDesc(path),
        path,
        icon: menu.icon
      })
    }
    if (Array.isArray(menu.children)) {
      result.push(...flattenScanMenus(menu.children as typeof menus))
    }
  }
  return result
}

onShow(async () => {
  uni.setNavigationBarTitle({ title: t('mobile.tab.scan') })
  if (!userStore.checkAuthGuard()) return

  try {
    const menus = await userStore.fetchMobileMenus()
    entries.value = flattenScanMenus(menus)
  } catch {
    entries.value = []
  }

  // 初版：无后端菜单时也展示工序报工入口，便于演示
  if (!entries.value.some((e) => e.path.includes('mes/report'))) {
    entries.value.unshift({
      title: t('mobile.report.title'),
      desc: t('mobile.scan.reportDesc'),
      path: '/pages/mes/report/index',
      icon: 'pass'
    })
  }
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
    <view class="hero">
      <view class="hero__icon-wrap">
        <u-icon name="scan" color="#ffffff" :size="28" />
      </view>
      <text class="hero__title">{{ t('mobile.scan.hubTitle') }}</text>
      <text class="hero__hint">{{ t('mobile.scan.hint') }}</text>
      <button class="hero__btn" @click="quickScan">{{ t('mobile.scan.startScan') }}</button>
    </view>

    <template v-if="entries.length">
      <text class="section-title">{{ t('mobile.scan.bizTitle') }}</text>
      <view class="list-card">
        <WfListRow
          v-for="(item, index) in entries"
          :key="item.path"
          :title="item.title"
          :desc="item.desc"
          :icon="item.icon"
          :path="item.path"
          :border="index < entries.length - 1"
          @click="openPage(item.path)"
        />
      </view>
    </template>

    <WfTabBar active="scan" />
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

.hero {
  background: linear-gradient(145deg, #3b82f6, $wf-primary-dark);
  border-radius: $wf-radius-lg;
  padding: 36rpx 32rpx 32rpx;
  color: #fff;
  margin-bottom: $wf-section-gap;
  box-shadow: $wf-shadow-hero;
}

.hero__icon-wrap {
  width: 72rpx;
  height: 72rpx;
  border-radius: 20rpx;
  background: rgba(255, 255, 255, 0.18);
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 20rpx;
}

.hero__title {
  display: block;
  font-size: 36rpx;
  font-weight: 700;
}

.hero__hint {
  display: block;
  margin-top: 10rpx;
  font-size: 24rpx;
  opacity: 0.88;
  line-height: 1.4;
}

.hero__btn {
  margin-top: 28rpx;
  height: 88rpx;
  line-height: 88rpx;
  background: #fff;
  color: $wf-primary;
  border-radius: $wf-radius-sm;
  font-size: 30rpx;
  font-weight: 600;
  border: none;
}

.hero__btn::after {
  border: none;
}

.section-title {
  display: block;
  margin-bottom: 16rpx;
  padding-left: 4rpx;
  font-size: 28rpx;
  font-weight: 600;
  color: #334155;
}

.list-card {
  background: $wf-card;
  border-radius: $wf-radius-md;
  overflow: hidden;
  box-shadow: $wf-shadow;
}
</style>
