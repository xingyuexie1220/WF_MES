import fs from 'node:fs'
import path from 'node:path'
import { fileURLToPath } from 'node:url'

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..')
const messagesDir = path.join(root, 'messages')
const outDir = path.join(root, '..', 'web', 'wf-mes-web', 'src', 'i18n', 'locales')

const locales = [
  { file: 'zh-CN.json', out: 'zh-CN.ts', exportName: 'zhCN' },
  { file: 'en.json', out: 'en.ts', exportName: 'en' },
  { file: 'zh-TW.json', out: 'zh-TW.ts', exportName: 'zhTW' }
]

for (const locale of locales) {
  const json = JSON.parse(fs.readFileSync(path.join(messagesDir, locale.file), 'utf8'))
  const content = `export default ${JSON.stringify(json, null, 2)} as const\n`
  fs.writeFileSync(path.join(outDir, locale.out), content, 'utf8')
  console.log(`Synced ${locale.file} -> ${locale.out}`)
}
