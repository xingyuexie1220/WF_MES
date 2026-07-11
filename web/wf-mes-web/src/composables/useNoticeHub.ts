import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { ElNotification } from 'element-plus'
import { onMounted, onUnmounted } from 'vue'
import { useNoticeStore } from '@/stores/notice/notice'
import type { NoticePushItem } from '@/types/system/notice'

let connection: ReturnType<HubConnectionBuilder['build']> | null = null

function resolveHubUrl() {
  return `${window.location.origin}/hubs/notice`
}

export function useNoticeHub() {
  const noticeStore = useNoticeStore()

  async function connect() {
    const token = localStorage.getItem('wf_access_token')
    if (!token) {
      return
    }

    if (connection) {
      return
    }

    connection = new HubConnectionBuilder()
      .withUrl(resolveHubUrl(), { accessTokenFactory: () => localStorage.getItem('wf_access_token') ?? '' })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build()

    connection.on('ReceiveNotice', (payload: NoticePushItem) => {
      noticeStore.pushMessage(payload)
      ElNotification({
        title: payload.title,
        message: payload.content,
        type: 'info',
        duration: 8000
      })
    })

    try {
      await connection.start()
      noticeStore.setConnected(true)
      await noticeStore.loadPublished()
    } catch {
      noticeStore.setConnected(false)
      connection = null
    }
  }

  async function disconnect() {
    if (connection) {
      await connection.stop()
      connection = null
      noticeStore.setConnected(false)
    }
  }

  onMounted(() => {
    void connect()
  })

  onUnmounted(() => {
    void disconnect()
  })

  return { connect, disconnect }
}
