<script setup lang="ts">
import { ref } from 'vue'
import type { TableInstance } from 'element-plus'

defineOptions({ name: 'WfTable', inheritAttrs: false })

withDefaults(
  defineProps<{
    /** 树形/少量数据时不撑满整页高度 */
    autoHeight?: boolean
  }>(),
  { autoHeight: false }
)

const tableRef = ref<TableInstance>()

defineExpose({ tableRef })
</script>

<template>
  <div class="wf-page__table-wrap" :class="{ 'is-auto-height': autoHeight }">
    <el-table
      ref="tableRef"
      class="wf-table"
      border
      stripe
      :height="autoHeight ? undefined : '100%'"
      v-bind="$attrs"
    >
      <slot />
      <template v-if="$slots.empty" #empty>
        <slot name="empty" />
      </template>
    </el-table>
  </div>
</template>
