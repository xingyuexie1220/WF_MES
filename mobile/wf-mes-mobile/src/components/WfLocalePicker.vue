<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import { useLocaleStore } from '@/stores/locale'
import type { AppLocale } from '@/i18n'

withDefaults(defineProps<{ showLabel?: boolean }>(), { showLabel: true })

const { t } = useI18n()
const localeStore = useLocaleStore()

function openPicker() {
  uni.showActionSheet({
    itemList: localeStore.localeOptions.map((item) => t(item.labelKey)),
    success(res) {
      const next = localeStore.localeOptions[res.tapIndex]?.value as AppLocale
      if (next) {
        localeStore.applyLocale(next)
      }
    }
  })
}
</script>

<template>
  <view class="locale-picker" :class="{ 'locale-picker--compact': !showLabel }" @click="openPicker">
    <text v-if="showLabel" class="label">{{ t('locale.title') }}</text>
    <text class="value">{{ t(localeStore.getLocaleLabel(localeStore.locale)) }}</text>
    <text class="arrow">›</text>
  </view>
</template>

<style scoped lang="scss">
.locale-picker {
  display: flex;
  align-items: center;
  width: 100%;
  gap: 12rpx;
  font-size: 30rpx;
  color: #0f172a;
}

.label {
  flex: 1;
  color: #0f172a;
}

.value {
  font-size: 24rpx;
  color: #64748b;
}

.arrow {
  color: #cbd5e1;
  font-size: 36rpx;
  line-height: 1;
}

.locale-picker--compact {
  width: auto;
  font-size: 26rpx;
  color: #64748b;

  .value {
    color: #2563eb;
    font-weight: 600;
    font-size: 26rpx;
  }

  .arrow {
    font-size: 28rpx;
  }
}
</style>
