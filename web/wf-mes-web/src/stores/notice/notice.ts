import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { NoticePushItem } from '@/types/system/notice'
import { getPublishedNoticesApi } from '@/api/system/notice'

export const useNoticeStore = defineStore('notice', () => {
  const messages = ref<NoticePushItem[]>([])
  const unreadCount = ref(0)
  const connected = ref(false)

  async function loadPublished() {
    messages.value = await getPublishedNoticesApi(30)
  }

  function pushMessage(item: NoticePushItem) {
    messages.value = [item, ...messages.value.filter((m) => m.id !== item.id)]
    unreadCount.value += 1
  }

  function markAllRead() {
    unreadCount.value = 0
  }

  function setConnected(value: boolean) {
    connected.value = value
  }

  return {
    messages,
    unreadCount,
    connected,
    loadPublished,
    pushMessage,
    markAllRead,
    setConnected
  }
})
