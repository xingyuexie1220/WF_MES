<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import { CirclePlusFilled, RefreshLeft, Search } from '@element-plus/icons-vue'

defineOptions({ name: 'WfPageToolbar' })

withDefaults(
  defineProps<{
    showSearch?: boolean
    showReset?: boolean
    showAdd?: boolean
    addLabel?: string
  }>(),
  {
    showSearch: true,
    showReset: true,
    showAdd: true
  }
)

const emit = defineEmits<{
  search: []
  reset: []
  add: []
}>()

const { t } = useI18n()
</script>

<template>
  <el-form-item class="wf-page-toolbar">
    <el-button v-if="showSearch" type="primary" :icon="Search" @click="emit('search')">{{ t('common.query') }}</el-button>
    <el-button v-if="showReset" :icon="RefreshLeft" @click="emit('reset')">{{ t('common.reset') }}</el-button>
    <el-button v-if="showAdd" type="success" :icon="CirclePlusFilled" @click="emit('add')">
      {{ addLabel || t('common.add') }}
    </el-button>
    <slot />
  </el-form-item>
</template>
