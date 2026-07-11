import request, { type AxiosRequestConfig, type AxiosResponse } from '@/utils/request/index'
import { parseFilenameFromDisposition, saveBlob } from '@/utils/download/blob'

export const http = {
  get<T>(url: string, config?: AxiosRequestConfig) {
    return request.get<any, T>(url, config)
  },
  post<T, B = unknown>(url: string, body?: B, config?: AxiosRequestConfig) {
    return request.post<any, T>(url, body, config)
  },
  put<T, B = unknown>(url: string, body?: B, config?: AxiosRequestConfig) {
    return request.put<any, T>(url, body, config)
  },
  delete<T>(url: string, config?: AxiosRequestConfig) {
    return request.delete<any, T>(url, config)
  },
  async download(url: string, params?: Record<string, unknown>, filename?: string) {
    const response = (await request.get(url, {
      params,
      responseType: 'blob',
      wfRawResponse: true
    } as AxiosRequestConfig)) as AxiosResponse<Blob>
    const resolvedName =
      filename ||
      parseFilenameFromDisposition(response.headers['content-disposition']) ||
      url.split('/').pop() ||
      'download.bin'
    saveBlob(response.data, resolvedName)
    return response.data
  }
}

export default http
