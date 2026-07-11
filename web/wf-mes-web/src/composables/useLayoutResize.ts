import { getCurrentInstance, onUnmounted, watch, type WatchStopHandle } from 'vue'
import { useMenuStore } from '@/stores/layout/menu'

export function useLayoutResize(callback: () => void, delayMs = 220) {
  const menuStore = useMenuStore()
  let timer: number | undefined
  let stopWatch: WatchStopHandle | undefined

  function run() {
    if (timer) {
      window.clearTimeout(timer)
    }
    timer = window.setTimeout(callback, delayMs)
  }

  if (getCurrentInstance()) {
    stopWatch = watch(() => menuStore.collapsed, run)
    onUnmounted(() => {
      stopWatch?.()
      if (timer) {
        window.clearTimeout(timer)
      }
    })
  }

  return { trigger: run }
}
