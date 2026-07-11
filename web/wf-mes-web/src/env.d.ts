/// <reference types="vite/client" />

declare module '*.png' {
  const src: string
  export default src
}

declare module 'print-js' {
  interface PrintOptions {
    printable: string | HTMLElement
    type?: 'pdf' | 'html' | 'image' | 'json' | 'raw-html'
    documentTitle?: string
    targetStyles?: string[]
    scanStyles?: boolean
  }
  export default function printJS(options: PrintOptions): void
}
