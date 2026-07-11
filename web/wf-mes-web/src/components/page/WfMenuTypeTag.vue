<script setup lang="ts">
import { computed } from 'vue'

defineOptions({ name: 'WfMenuTypeTag' })

const props = defineProps<{
  type: number
  label: string
}>()

const typeClass = computed(() => {
  if (props.type === 1) return 'is-dir'
  if (props.type === 2) return 'is-menu'
  return 'is-button'
})
</script>

<template>
  <span class="wf-menu-type-tag" :class="typeClass">{{ label }}</span>
</template>

<style scoped lang="scss">
.wf-menu-type-tag {
  display: inline-block;
  padding: 0 8px;
  height: 22px;
  line-height: 22px;
  border-radius: 4px;
  font-size: 12px;
  font-weight: 500;
  white-space: nowrap;

  &.is-dir {
    color: #2d8cf0;
    background: rgba(45, 140, 240, 0.08);
  }

  &.is-menu {
    color: #52a06b;
    background: rgba(82, 160, 107, 0.08);
  }

  &.is-button {
    color: #909399;
    background: rgba(144, 147, 153, 0.1);
  }
}
</style>
