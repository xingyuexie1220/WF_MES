<script setup lang="ts">
import { useI18n } from 'vue-i18n'

defineOptions({ name: 'WfStatusTag' })

defineProps<{
  status: number
}>()

const { t } = useI18n()
</script>

<template>
  <el-tag :type="status === 1 ? 'success' : 'info'" size="small">
    {{ status === 1 ? t('common.enabled') : t('common.disabled') }}
  </el-tag>
</template>
