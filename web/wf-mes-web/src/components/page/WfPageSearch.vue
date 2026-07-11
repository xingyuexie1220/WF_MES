<script setup lang="ts">
import WfPageToolbar from './WfPageToolbar.vue'

defineOptions({ name: 'WfPageSearch' })

withDefaults(
  defineProps<{
    showSearch?: boolean
    showReset?: boolean
    showAdd?: boolean
    addLabel?: string
    hideActions?: boolean
  }>(),
  {
    showSearch: true,
    showReset: true,
    showAdd: true,
    hideActions: false
  }
)

const emit = defineEmits<{
  search: []
  reset: []
  add: []
}>()
</script>

<template>
  <el-card shadow="never" class="wf-page__search">
    <el-form inline @submit.prevent="emit('search')">
      <slot />
      <WfPageToolbar
        v-if="!hideActions"
        :show-search="showSearch"
        :show-reset="showReset"
        :show-add="showAdd"
        :add-label="addLabel"
        @search="emit('search')"
        @reset="emit('reset')"
        @add="emit('add')"
      >
        <slot name="actions" />
      </WfPageToolbar>
    </el-form>
  </el-card>
</template>
