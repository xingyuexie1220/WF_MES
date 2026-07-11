import JSZip from 'jszip'
import { saveBlob } from '@/utils/download/blob'

export interface ZipEntry {
  filename: string
  content: Blob | string | ArrayBuffer
}

export async function downloadZip(filename: string, entries: ZipEntry[]) {
  const zip = new JSZip()
  for (const entry of entries) {
    zip.file(entry.filename, entry.content)
  }
  const blob = await zip.generateAsync({ type: 'blob' })
  saveBlob(blob, filename.endsWith('.zip') ? filename : `${filename}.zip`)
  return blob
}
