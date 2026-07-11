<script setup lang="ts">
import { onLaunch } from '@dcloudio/uni-app'
import { installRouteGuard } from '@/utils/route-guard'
import { useLocaleStore } from '@/stores/locale'

onLaunch(() => {
  installRouteGuard()
  useLocaleStore().applyLocale(useLocaleStore().locale)
})
</script>

<template>
  <view class="app-root">
    <slot />
  </view>
</template>

<style lang="scss">
@import 'uview-plus/index.scss';

.app-root {
  min-height: 100vh;
  background: #f1f5f9;
}
</style>
