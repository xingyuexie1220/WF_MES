import dayjs from 'dayjs'
import relativeTime from 'dayjs/plugin/relativeTime'
import 'dayjs/locale/zh-cn'

dayjs.extend(relativeTime)
dayjs.locale('zh-cn')

const DEFAULT_DATE = 'YYYY-MM-DD'
const DEFAULT_DATETIME = 'YYYY-MM-DD HH:mm:ss'

export function formatDate(value?: string | number | Date | null, pattern = DEFAULT_DATE) {
  if (value == null || value === '') {
    return ''
  }
  const date = dayjs(value)
  return date.isValid() ? date.format(pattern) : ''
}

export function formatDateTime(value?: string | number | Date | null, pattern = DEFAULT_DATETIME) {
  return formatDate(value, pattern)
}

export function fromNow(value?: string | number | Date | null) {
  if (value == null || value === '') {
    return ''
  }
  const date = dayjs(value)
  return date.isValid() ? date.fromNow() : ''
}

export function timestampForFilename(value?: string | number | Date | null) {
  return formatDateTime(value ?? new Date(), 'YYYYMMDDHHmmss')
}
