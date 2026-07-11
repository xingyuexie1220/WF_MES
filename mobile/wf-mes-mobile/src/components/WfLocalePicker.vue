<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import { useLocaleStore } from '@/stores/locale'
import type { AppLocale } from '@/i18n'

const props = withDefaults(defineProps<{ showLabel?: boolean }>(), { showLabel: true })

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
  <view class="locale-picker" @click="openPicker">
    <text v-if="showLabel" class="label">{{ t('locale.title') }}</text>
    <text class="value">{{ t(localeStore.getLocaleLabel(localeStore.locale)) }}</text>
    <text class="arrow">›</text>
  </view>
</template>

<style scoped lang="scss">
.locale-picker {
  display: flex;
  align-items: center;
  gap: 12rpx;
  font-size: 26rpx;
  color: #64748b;
}

.label {
  color: #94a3b8;
}

.value {
  color: #2563eb;
  font-weight: 600;
}

.arrow {
  color: #cbd5e1;
}
</style>
