<script setup lang="ts">
defineOptions({ name: 'WfPagePager' })

const currentPage = defineModel<number>('currentPage', { required: true })
const pageSize = defineModel<number>('pageSize', { required: true })

withDefaults(
  defineProps<{
    total: number
    pageSizes?: number[]
    layout?: string
  }>(),
  {
    pageSizes: () => [20, 30, 50],
    layout: 'total, sizes, prev, pager, next'
  }
)

const emit = defineEmits<{
  change: []
  sizeChange: []
}>()
</script>

<template>
  <el-pagination
    v-model:current-page="currentPage"
    v-model:page-size="pageSize"
    :total="total"
    :page-sizes="pageSizes"
    :layout="layout"
    @current-change="emit('change')"
    @size-change="emit('sizeChange')"
  />
</template>
