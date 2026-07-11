<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { useNoticeStore } from '@/stores/notice/notice'

const visible = defineModel<boolean>({ default: false })
const { t } = useI18n()
const noticeStore = useNoticeStore()

const messages = computed(() => noticeStore.messages)

function formatTime(value?: string) {
  if (!value) return ''
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return value
  const hh = String(date.getHours()).padStart(2, '0')
  const mm = String(date.getMinutes()).padStart(2, '0')
  return `${hh}:${mm}`
}

function handleOpen() {
  noticeStore.markAllRead()
}
</script>

<template>
  <el-drawer v-model="visible" :title="t('layout.messages')" size="360px" append-to-body @open="handleOpen">
    <div class="wf-message-drawer">
      <div v-if="!messages.length" class="wf-message-drawer__empty">{{ t('system.notice.emptyPublished') }}</div>
      <div v-for="item in messages" :key="item.id" class="wf-message-drawer__item">
        <div class="wf-message-drawer__title">
          <span>{{ item.title }}</span>
          <span class="wf-message-drawer__time">{{ formatTime(item.publishTime) }}</span>
        </div>
        <div class="wf-message-drawer__content">{{ item.content }}</div>
      </div>
    </div>
  </el-drawer>
</template>

<style scoped lang="scss">
.wf-message-drawer {
  display: flex;
  flex-direction: column;
  gap: 16px;

  &__empty {
    color: var(--wf-text-muted);
    font-size: 13px;
    text-align: center;
    padding: 24px 0;
  }

  &__item {
    padding-bottom: 16px;
    border-bottom: 1px dashed var(--wf-border);
  }

  &__title {
    display: flex;
    justify-content: space-between;
    gap: 12px;
    font-size: 14px;
    color: var(--wf-text);
    margin-bottom: 6px;
  }

  &__time {
    color: var(--wf-text-muted);
    font-size: 12px;
    flex-shrink: 0;
  }

  &__content {
    color: var(--wf-text-secondary);
    font-size: 13px;
    line-height: 1.6;
    white-space: pre-wrap;
  }
}
</style>
