<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { onShow } from '@dcloudio/uni-app'

const props = withDefaults(
  defineProps<{
    /** 当前选中：home | scan | mine */
    active?: 'home' | 'scan' | 'mine'
  }>(),
  { active: 'home' }
)

const { t, locale } = useI18n()
const current = ref(props.active)

watch(
  () => props.active,
  (v) => {
    current.value = v
  }
)

onShow(() => {
  current.value = props.active
  // 自定义 Tab 时隐藏原生栏，避免再出现色块占位
  uni.hideTabBar({ animation: false })
})

const tabs = computed(() => [
  {
    key: 'home' as const,
    path: '/pages/home/index',
    text: t('mobile.tab.home'),
    icon: 'home',
    iconActive: 'home-fill'
  },
  {
    key: 'scan' as const,
    path: '/pages/scan/index',
    text: t('mobile.tab.scan'),
    icon: 'scan',
    iconActive: 'scan'
  },
  {
    key: 'mine' as const,
    path: '/pages/mine/index',
    text: t('mobile.tab.mine'),
    icon: 'account',
    iconActive: 'account-fill'
  }
])

// locale 变化时刷新文案
void locale

function onTap(key: 'home' | 'scan' | 'mine', path: string) {
  if (current.value === key) return
  current.value = key
  uni.switchTab({ url: path })
}
</script>

<template>
  <view class="wf-tabbar-spacer" />
  <view class="wf-tabbar">
    <view
      v-for="tab in tabs"
      :key="tab.key"
      class="wf-tabbar__item"
      @click="onTap(tab.key, tab.path)"
    >
      <u-icon
        :name="current === tab.key ? tab.iconActive : tab.icon"
        :color="current === tab.key ? '#2563eb' : '#8a94a6'"
        :size="22"
      />
      <text class="wf-tabbar__text" :class="{ 'wf-tabbar__text--active': current === tab.key }">
        {{ tab.text }}
      </text>
    </view>
  </view>
</template>

<style scoped lang="scss">
.wf-tabbar-spacer {
  height: calc(100rpx + env(safe-area-inset-bottom));
  width: 100%;
}

.wf-tabbar {
  position: fixed;
  left: 0;
  right: 0;
  bottom: 0;
  z-index: 900;
  display: flex;
  align-items: stretch;
  height: calc(100rpx + env(safe-area-inset-bottom));
  padding-bottom: env(safe-area-inset-bottom);
  background: #ffffff;
  border-top: 1rpx solid #eef2f7;
  box-sizing: border-box;
}

.wf-tabbar__item {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 4rpx;
  padding-top: 8rpx;
}

.wf-tabbar__text {
  font-size: 20rpx;
  color: #8a94a6;
  line-height: 1.2;

  &--active {
    color: #2563eb;
    font-weight: 600;
  }
}
</style>
