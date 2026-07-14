<script setup lang="ts">
import WfMenuIcon from '@/components/WfMenuIcon.vue'

withDefaults(
  defineProps<{
    title: string
    desc?: string
    value?: string
    tag?: string
    icon?: string
    path?: string
    /** 直接指定 uView 图标名（设置页等） */
    iconName?: string
    iconBg?: string
    iconFg?: string
    showArrow?: boolean
    muted?: boolean
    border?: boolean
  }>(),
  {
    showArrow: true,
    muted: false,
    border: false
  }
)

const emit = defineEmits<{ click: [] }>()
</script>

<template>
  <view
    class="wf-list-row"
    :class="{ 'wf-list-row--muted': muted, 'wf-list-row--border': border }"
    @click="emit('click')"
  >
    <WfMenuIcon
      :icon="icon"
      :path="path"
      :name="iconName"
      :bg="iconBg"
      :fg="iconFg"
      size="lg"
    />
    <view class="wf-list-row__body">
      <view class="wf-list-row__title-row">
        <text class="wf-list-row__title">{{ title }}</text>
        <text v-if="tag" class="wf-list-row__tag">{{ tag }}</text>
      </view>
      <text v-if="desc" class="wf-list-row__desc">{{ desc }}</text>
    </view>
    <text v-if="value" class="wf-list-row__value">{{ value }}</text>
    <text v-if="showArrow" class="wf-list-row__chevron">›</text>
  </view>
</template>

<style scoped lang="scss">
@import '@/styles/tokens.scss';

.wf-list-row {
  display: flex;
  align-items: center;
  gap: 20rpx;
  min-height: $wf-row-min-height;
  padding: 24rpx 28rpx;
  background: $wf-card;
  box-sizing: border-box;

  &--border {
    border-bottom: 1rpx solid #f1f5f9;

    &:last-child {
      border-bottom: none;
    }
  }

  &--muted {
    opacity: 0.72;
  }

  &:active {
    background: #f8fafc;
  }

  &__body {
    flex: 1;
    min-width: 0;
  }

  &__title-row {
    display: flex;
    align-items: center;
    gap: 12rpx;
  }

  &__title {
    font-size: 30rpx;
    font-weight: 600;
    color: $wf-text;
  }

  &__tag {
    padding: 2rpx 10rpx;
    border-radius: 8rpx;
    background: $wf-bg;
    color: $wf-text-muted;
    font-size: 20rpx;
    flex-shrink: 0;
  }

  &__desc {
    display: block;
    margin-top: 6rpx;
    font-size: 22rpx;
    color: $wf-text-muted;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }

  &__value {
    max-width: 280rpx;
    font-size: 24rpx;
    color: $wf-text-secondary;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    flex-shrink: 0;
  }

  &__chevron {
    color: #cbd5e1;
    font-size: 36rpx;
    line-height: 1;
    flex-shrink: 0;
  }
}
</style>
