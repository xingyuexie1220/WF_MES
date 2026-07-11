<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
import JsBarcode from 'jsbarcode'

const props = withDefaults(
  defineProps<{
    value: string
    format?: string
    width?: number
    height?: number
    displayValue?: boolean
  }>(),
  {
    format: 'CODE128',
    width: 2,
    height: 64,
    displayValue: true
  }
)

const svgRef = ref<SVGSVGElement | null>(null)

function renderBarcode() {
  if (!svgRef.value || !props.value) {
    return
  }
  JsBarcode(svgRef.value, props.value, {
    format: props.format,
    width: props.width,
    height: props.height,
    displayValue: props.displayValue,
    margin: 8
  })
}

onMounted(renderBarcode)
watch(() => [props.value, props.format, props.width, props.height, props.displayValue], renderBarcode)
</script>

<template>
  <div class="wf-barcode-preview">
    <svg v-if="value" ref="svgRef" class="wf-barcode-preview__svg" />
    <span v-else class="wf-barcode-preview__empty">—</span>
  </div>
</template>

<style scoped lang="scss">
.wf-barcode-preview {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-height: 72px;

  &__svg {
    max-width: 100%;
  }

  &__empty {
    color: var(--el-text-color-secondary);
  }
}
</style>
