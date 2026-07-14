<script setup lang="ts">
import { computed } from 'vue'
import wfLogoMark from '@/assets/images/wf-logo-mark.svg?raw'

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

/** 内联 SVG，用 currentColor 适配浅/深底，避免白描边在白底上「缺角」。 */
const logoHtml = computed(() => wfLogoMark)
</script>

<template>
  <div class="wf-brand" :class="{ 'wf-brand--mini': mini, [`wf-brand--${theme}`]: true }">
    <span
      class="wf-brand__logo"
      :style="{ width: `${size}px`, height: `${size}px` }"
      v-html="logoHtml"
      aria-hidden="true"
    />
    <span v-if="showText && !mini" class="wf-brand__text">MES</span>
  </div>
</template>

<style scoped lang="scss">
.wf-brand {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  color: #ffffff;

  &--mini {
    justify-content: center;
  }

  &__logo {
    display: inline-flex;
    flex-shrink: 0;
    color: inherit;

    :deep(svg) {
      width: 100%;
      height: 100%;
      display: block;
    }
  }

  &__text {
    color: inherit;
    font-size: 21px;
    font-weight: 700;
    letter-spacing: 2px;
    line-height: 1;
  }

  &--dark {
    color: #2d8cf0;
  }

  &--dark &__text {
    color: #323233;
  }
}
</style>
