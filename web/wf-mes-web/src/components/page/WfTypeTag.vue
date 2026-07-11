<script setup lang="ts">
import { computed } from 'vue'

defineOptions({ name: 'WfTypeTag' })

const props = defineProps<{
  type: number
  label: string
}>()

const typeClass = computed(() => {
  if (props.type === 1) return 'is-group'
  if (props.type === 2) return 'is-company'
  return 'is-dept'
})
</script>

<template>
  <span class="wf-type-tag" :class="typeClass">{{ label }}</span>
</template>

<style scoped lang="scss">
.wf-type-tag {
  display: inline-block;
  padding: 0 8px;
  height: 22px;
  line-height: 22px;
  border-radius: 4px;
  font-size: 12px;
  font-weight: 500;

  &.is-group {
    color: #2d8cf0;
    background: rgba(45, 140, 240, 0.08);
  }

  &.is-company {
    color: #646cff;
    background: rgba(100, 108, 255, 0.08);
  }

  &.is-dept {
    color: #52a06b;
    background: rgba(82, 160, 107, 0.08);
  }
}
</style>
