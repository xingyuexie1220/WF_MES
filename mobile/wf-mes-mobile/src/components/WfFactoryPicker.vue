<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import type { FactorySummary } from '@/types/auth'

const props = defineProps<{
  show: boolean
  factories: FactorySummary[]
  currentFactoryId?: number
}>()

const emit = defineEmits<{
  close: []
  select: [factory: FactorySummary]
}>()

const { t } = useI18n()
const visible = computed(() => props.show)

function handleSelect(factory: FactorySummary) {
  emit('select', factory)
}
</script>

<template>
  <u-popup :show="visible" mode="bottom" round="16" @close="emit('close')">
    <view class="factory-picker">
      <view class="title">{{ t('mobile.factory.selectTitle') }}</view>
      <view class="hint">{{ t('mobile.factory.selectHint') }}</view>
      <view
        v-for="item in factories"
        :key="item.id"
        class="factory-item"
        :class="{ active: item.id === currentFactoryId }"
        @click="handleSelect(item)"
      >
        <view class="factory-name">{{ item.factoryName }}</view>
        <view class="factory-meta">
          <text>{{ item.factoryCode }}</text>
          <text v-if="item.isDefault" class="tag">{{ t('mobile.factory.defaultTag') }}</text>
        </view>
      </view>
    </view>
  </u-popup>
</template>

<style scoped lang="scss">
.factory-picker {
  padding: 32rpx 32rpx 48rpx;
}

.title {
  font-size: 34rpx;
  font-weight: 700;
  color: #0f172a;
}

.hint {
  margin-top: 12rpx;
  margin-bottom: 24rpx;
  font-size: 24rpx;
  color: #64748b;
  line-height: 1.5;
}

.factory-item {
  padding: 28rpx 0;
  border-bottom: 1rpx solid #f1f5f9;
}

.factory-item.active .factory-name {
  color: #2563eb;
}

.factory-name {
  font-size: 30rpx;
  font-weight: 600;
  color: #0f172a;
}

.factory-meta {
  margin-top: 8rpx;
  display: flex;
  align-items: center;
  gap: 12rpx;
  font-size: 22rpx;
  color: #94a3b8;
}

.tag {
  padding: 2rpx 12rpx;
  border-radius: 999rpx;
  background: #dbeafe;
  color: #2563eb;
}
</style>
