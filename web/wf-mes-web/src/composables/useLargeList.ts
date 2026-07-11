import { markRaw, shallowRef } from 'vue'

export function useLargeList<T extends object>() {
  const items = shallowRef<T[]>([])
  const loading = shallowRef(false)

  function reset(nextItems: T[] = [], raw = true) {
    items.value = raw ? nextItems.map((item) => markRaw(item)) : nextItems
  }

  function appendPage(pageItems: T[], raw = true) {
    const mapped = raw ? pageItems.map((item) => markRaw(item)) : pageItems
    items.value = [...items.value, ...mapped]
  }

  function setLoading(value: boolean) {
    loading.value = value
  }

  return {
    items,
    loading,
    reset,
    appendPage,
    setLoading
  }
}
