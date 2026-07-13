import fs from 'node:fs'
import path from 'node:path'
import { fileURLToPath } from 'node:url'

const repoRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..', '..')
const apiCodesDir = path.join(repoRoot, 'i18n', 'api-codes')
const locales = ['zh-CN', 'zh-TW', 'en']

const targets = locales.map((locale) => ({
  locale,
  path: path.join(repoRoot, 'desktop', 'WF.MES.WPF', 'i18n', `${locale}.json`)
})).concat(locales.map((locale) => ({
  locale,
  path: path.join(repoRoot, 'mobile', 'wf-mes-mobile', 'src', 'i18n', 'locales', `${locale}.json`)
})))

function deepMerge(target, source) {
  for (const [key, value] of Object.entries(source)) {
    if (key === '_comment') continue
    if (value && typeof value === 'object' && !Array.isArray(value)) {
      target[key] = deepMerge(target[key] ?? {}, value)
    } else {
      target[key] = value
    }
  }
  return target
}

for (const target of targets) {
  const apiPath = path.join(apiCodesDir, `${target.locale}.json`)
  if (!fs.existsSync(apiPath)) {
    console.warn(`Skip missing api-codes: ${apiPath}`)
    continue
  }
  const apiJson = JSON.parse(fs.readFileSync(apiPath, 'utf8'))
  const existing = fs.existsSync(target.path)
    ? JSON.parse(fs.readFileSync(target.path, 'utf8'))
    : {}
  const merged = deepMerge(existing, apiJson)
  fs.writeFileSync(target.path, `${JSON.stringify(merged, null, 2)}\n`, 'utf8')
  console.log(`Merged api-codes -> ${path.relative(repoRoot, target.path)}`)
}

console.log('Done. Web locales must be updated manually in web/wf-mes-web/src/i18n/locales/')
