<script setup lang="ts">
import wfLogoMark from '@/assets/images/wf-logo-mark.svg'

withDefaults(
  defineProps<{
    showText?: boolean
    mini?: boolean
    size?: number
    theme?: 'light' | 'dark'
  }>(),
  {
    showText: true,
    mini: false,
    size: 36,
    theme: 'light'
  }
)
</script>

<template>
  <div class="wf-brand" :class="{ 'wf-brand--mini': mini, [`wf-brand--${theme}`]: true }">
    <img :src="wfLogoMark" class="wf-brand__logo" alt="WF" :style="{ width: `${size}px`, height: `${size}px` }" />
    <span v-if="showText && !mini" class="wf-brand__text">MES</span>
  </div>
</template>

<style scoped lang="scss">
.wf-brand {
  display: inline-flex;
  align-items: center;
  gap: 8px;

  &--mini {
    justify-content: center;
  }

  &__logo {
    object-fit: contain;
    flex-shrink: 0;
  }

  &__text {
    color: #fff;
    font-size: 21px;
    font-weight: 700;
    letter-spacing: 2px;
    line-height: 1;
  }

  &--dark &__text {
    color: #323233;
  }
}
</style>
