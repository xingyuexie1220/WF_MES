<script setup lang="ts">
import { computed } from 'vue'
import { resolveMenuIcon } from '@/utils/menuIcon'

const props = withDefaults(
  defineProps<{
    icon?: string
    path?: string
    size?: 'sm' | 'md' | 'lg'
    /** 覆盖解析结果，用于设置页等固定图标 */
    name?: string
    bg?: string
    fg?: string
  }>(),
  { size: 'md' }
)

const style = computed(() => {
  const resolved = resolveMenuIcon(props.icon, props.path)
  return {
    bg: props.bg || resolved.bg,
    fg: props.fg || resolved.fg,
    name: props.name || resolved.name
  }
})

const iconPx = computed(() => {
  if (props.size === 'lg') return 22
  if (props.size === 'sm') return 16
  return 18
})
</script>

<template>
  <view
    class="wf-menu-icon"
    :class="`wf-menu-icon--${size}`"
    :style="{ background: style.bg }"
  >
    <u-icon :name="style.name" :color="style.fg" :size="iconPx" />
  </view>
</template>

<style scoped lang="scss">
.wf-menu-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  border-radius: 16rpx;

  &--sm {
    width: 56rpx;
    height: 56rpx;
    border-radius: 12rpx;
  }

  &--md {
    width: 72rpx;
    height: 72rpx;
  }

  &--lg {
    width: 80rpx;
    height: 80rpx;
    border-radius: 18rpx;
  }
}
</style>
