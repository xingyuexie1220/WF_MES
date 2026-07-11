<script setup lang="ts">
import { ref } from 'vue'

defineOptions({ name: 'WfVxeTable', inheritAttrs: false })

withDefaults(
  defineProps<{
    autoHeight?: boolean
    rowHeight?: number
  }>(),
  {
    autoHeight: false,
    rowHeight: 44
  }
)

const tableRef = ref()

defineExpose({ tableRef })
</script>

<template>
  <div class="wf-page__table-wrap" :class="{ 'is-auto-height': autoHeight }">
    <vxe-table
      ref="tableRef"
      class="wf-vxe-table"
      border
      stripe
      size="small"
      :height="autoHeight ? undefined : '100%'"
      :row-config="{ isHover: true, height: rowHeight }"
      :scroll-y="{ enabled: true, gt: 20 }"
      v-bind="$attrs"
    >
      <slot />
      <template v-if="$slots.empty" #empty>
        <slot name="empty" />
      </template>
    </vxe-table>
  </div>
</template>

<style scoped lang="scss">
.wf-vxe-table {
  width: 100%;
}
</style>
