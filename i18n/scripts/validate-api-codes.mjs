import fs from 'node:fs'
import path from 'node:path'
import { fileURLToPath } from 'node:url'

const repoRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..', '..')

function flatten(obj, prefix = '') {
  const out = {}
  for (const [key, value] of Object.entries(obj)) {
    if (key === '_comment') continue
    const full = prefix ? `${prefix}.${key}` : key
    if (value && typeof value === 'object' && !Array.isArray(value)) {
      Object.assign(out, flatten(value, full))
    } else {
      out[full] = value
    }
  }
  return out
}

function readWfMessageCodes() {
  const csPath = path.join(repoRoot, 'backend', 'WF.MES.Shared', 'Constants', 'WfMessageCodes.cs')
  const text = fs.readFileSync(csPath, 'utf8')
  const codes = [...text.matchAll(/=\s*"([^"]+)"/g)].map((m) => m[1])
  return [...new Set(codes)]
}

function loadJsonFlat(filePath) {
  if (!fs.existsSync(filePath)) return null
  return flatten(JSON.parse(fs.readFileSync(filePath, 'utf8')))
}

function loadTsFlat(filePath) {
  if (!fs.existsSync(filePath)) return null
  const text = fs.readFileSync(filePath, 'utf8')
  const stripped = text
    .replace(/^\s*export\s+default\s+/, '')
    .replace(/\s+as\s+const\s*;?\s*$/, '')
    .trim()
    .replace(/;?\s*$/, '')
  return flatten(new Function(`return (${stripped})`)())
}

const codes = readWfMessageCodes()
const packs = [
  { name: 'api-codes (zh-CN)', flat: loadJsonFlat(path.join(repoRoot, 'i18n', 'api-codes', 'zh-CN.json')) },
  { name: 'desktop (zh-CN)', flat: loadJsonFlat(path.join(repoRoot, 'desktop', 'WF.MES.WPF', 'i18n', 'zh-CN.json')) },
  { name: 'mobile (zh-CN)', flat: loadJsonFlat(path.join(repoRoot, 'mobile', 'wf-mes-mobile', 'src', 'i18n', 'locales', 'zh-CN.json')) },
  { name: 'web (zh-CN)', flat: loadTsFlat(path.join(repoRoot, 'web', 'wf-mes-web', 'src', 'i18n', 'locales', 'zh-CN.ts')) }
]

let failed = false
for (const code of codes) {
  for (const pack of packs) {
    if (!pack.flat || !(code in pack.flat)) {
      console.error(`MISSING [${pack.name}] key: ${code}`)
      failed = true
    }
  }
}

if (failed) {
  process.exit(1)
}
console.log(`OK: all ${codes.length} WfMessageCodes present in api-codes, desktop, mobile, and web (zh-CN).`)
