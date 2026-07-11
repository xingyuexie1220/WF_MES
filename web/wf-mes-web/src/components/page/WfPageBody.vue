<template>
  <el-card shadow="never" class="wf-page__body">
    <slot />
    <div v-if="$slots.pager" class="wf-page__pager">
      <slot name="pager" />
    </div>
  </el-card>
</template>

<script setup lang="ts">
defineOptions({ name: 'WfPageBody' })
</script>
