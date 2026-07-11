<script setup lang="ts">
import { useI18n } from 'vue-i18n'

defineOptions({ name: 'WfDialogFooter' })

withDefaults(
  defineProps<{
    confirmText?: string
    loading?: boolean
  }>(),
  {
    loading: false
  }
)

const emit = defineEmits<{
  cancel: []
  confirm: []
}>()

const { t } = useI18n()
</script>

<template>
  <el-button @click="emit('cancel')">{{ t('common.cancel') }}</el-button>
  <el-button type="primary" :loading="loading" @click="emit('confirm')">
    {{ confirmText || t('common.save') }}
  </el-button>
</template>
