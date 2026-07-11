import * as XLSX from 'xlsx'

export interface ExcelColumn<T> {
  header: string
  key?: keyof T
  width?: number
  formatter?: (row: T) => string | number
}

export function exportExcel<T extends object>(
  filename: string,
  sheetName: string,
  columns: ExcelColumn<T>[],
  rows: T[]
) {
  const header = columns.map((col) => col.header)
  const data = rows.map((row) =>
    columns.map((col) => {
      if (col.formatter) {
        return col.formatter(row)
      }
      if (col.key) {
        const value = row[col.key]
        return value ?? ''
      }
      return ''
    })
  )

  const worksheet = XLSX.utils.aoa_to_sheet([header, ...data])
  worksheet['!cols'] = columns.map((col) => ({ wch: col.width ?? 16 }))

  const workbook = XLSX.utils.book_new()
  XLSX.utils.book_append_sheet(workbook, worksheet, sheetName.slice(0, 31))
  XLSX.writeFile(workbook, filename.endsWith('.xlsx') ? filename : `${filename}.xlsx`)
}
