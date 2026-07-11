import { ref } from 'vue'
import { getDictOptionsApi } from '@/api/system/dict'
import type { DictDataItem } from '@/types/system/dict'

const cache = new Map<string, DictDataItem[]>()

export function invalidateDictCache(dictType?: string) {
  if (dictType) {
    cache.delete(dictType)
    return
  }
  cache.clear()
}

export function useDict(dictType: string) {
  const options = ref<DictDataItem[]>([])
  const loading = ref(false)

  async function load(refresh = false) {
    if (!refresh && cache.has(dictType)) {
      options.value = [...cache.get(dictType)!]
      return options.value
    }

    loading.value = true
    try {
      const data = await getDictOptionsApi(dictType)
      cache.set(dictType, data)
      options.value = data
      return data
    } finally {
      loading.value = false
    }
  }

  function labelOf(value: string | number | undefined | null) {
    if (value === undefined || value === null) return ''
    const key = String(value)
    return options.value.find((item) => item.dictValue === key)?.dictLabel ?? key
  }

  function defaultValue(fallback: string | number = '') {
    return options.value[0] ? Number(options.value[0].dictValue) : Number(fallback)
  }

  return { options, loading, load, labelOf, defaultValue, invalidate: () => invalidateDictCache(dictType) }
}
