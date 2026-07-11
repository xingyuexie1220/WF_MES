import printJS from 'print-js'

export function printLabelElement(element: HTMLElement | string, title = '条码标签') {
  printJS({
    printable: element,
    type: 'html',
    documentTitle: title,
    targetStyles: ['*'],
    scanStyles: false
  })
}

export function printLabelHtml(html: string, title = '条码标签') {
  printJS({
    printable: html,
    type: 'raw-html',
    documentTitle: title,
    targetStyles: ['*']
  })
}
