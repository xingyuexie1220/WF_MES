<script setup lang="ts">
import { computed, ref } from 'vue'
import { onShow } from '@dcloudio/uni-app'
import { useI18n } from 'vue-i18n'
import { request } from '@/utils/request'
import { useUserStore } from '@/stores/user'

interface ProgressItem {
  processCode: string
  processName?: string
  seq: number
  goodQty: number
  defectQty: number
  remainQty: number
}

interface WorkOrderDetail {
  workOrderNo: string
  materialNo: string
  materialName?: string
  planQty: number
  status: string
  progress: ProgressItem[]
}

interface MachineItem {
  machineNo: string
  machineName: string
}

interface DefectItem {
  defectCode: string
  defectName: string
}

const { t } = useI18n()
const userStore = useUserStore()

const workOrderNo = ref('')
const detail = ref<WorkOrderDetail | null>(null)
const processCode = ref('')
const goodQty = ref(1)
const defectQty = ref(0)
const machineNo = ref('')
const defectCode = ref('')
const machines = ref<MachineItem[]>([])
const defects = ref<DefectItem[]>([])
const submitting = ref(false)
const recent = ref<Array<Record<string, unknown>>>([])

const processOptions = computed(() => detail.value?.progress || [])
const currentRemain = computed(() => {
  const p = processOptions.value.find((x) => x.processCode === processCode.value)
  return p?.remainQty ?? 0
})
const showMachine = computed(() => processCode.value === 'P20')
const showDefect = computed(() => defectQty.value > 0)

onShow(async () => {
  uni.setNavigationBarTitle({ title: t('mobile.report.title') })
  if (!userStore.checkAuthGuard()) return
  try {
    machines.value = await request<MachineItem[]>('/production/machines', 'GET')
    defects.value = await request<DefectItem[]>('/production/defect-codes', 'GET')
  } catch {
    machines.value = []
    defects.value = []
  }
})

async function loadWorkOrder() {
  const no = workOrderNo.value.trim()
  if (!no) {
    uni.showToast({ title: t('mobile.report.needWo'), icon: 'none' })
    return
  }
  try {
    detail.value = await request<WorkOrderDetail>(`/production/work-orders/${encodeURIComponent(no)}`, 'GET')
    if (!detail.value) {
      uni.showToast({ title: t('mobile.report.woNotFound'), icon: 'none' })
      return
    }
    const first = detail.value.progress.find((p) => p.remainQty > 0) || detail.value.progress[0]
    processCode.value = first?.processCode || ''
    goodQty.value = 1
    defectQty.value = 0
    defectCode.value = ''
  } catch {
    detail.value = null
  }
}

function handleScan() {
  uni.scanCode({
    success: async (res) => {
      workOrderNo.value = res.result
      await loadWorkOrder()
    }
  })
}

async function submitReport() {
  if (!workOrderNo.value.trim() || !processCode.value) {
    uni.showToast({ title: t('mobile.report.needProcess'), icon: 'none' })
    return
  }
  if (goodQty.value + defectQty.value <= 0) {
    uni.showToast({ title: t('mobile.report.needQty'), icon: 'none' })
    return
  }
  if (defectQty.value > 0 && !defectCode.value) {
    uni.showToast({ title: t('mobile.report.needDefect'), icon: 'none' })
    return
  }
  submitting.value = true
  try {
    const data = await request<Record<string, unknown>>('/production/pass-records', 'POST', {
      workOrderNo: workOrderNo.value.trim(),
      processCode: processCode.value,
      goodQty: goodQty.value,
      defectQty: defectQty.value,
      defectCode: defectCode.value || undefined,
      machineNo: machineNo.value || undefined
    })
    recent.value.unshift(data)
    uni.showToast({ title: t('common.success'), icon: 'success' })
    goodQty.value = 1
    defectQty.value = 0
    defectCode.value = ''
    await loadWorkOrder()
  } finally {
    submitting.value = false
  }
}

function onProcessPick(e: { detail: { value: string } }) {
  const idx = Number(e.detail.value)
  processCode.value = processOptions.value[idx]?.processCode || ''
}

function onMachinePick(e: { detail: { value: string } }) {
  const idx = Number(e.detail.value)
  machineNo.value = machines.value[idx]?.machineNo || ''
}

function onDefectPick(e: { detail: { value: string } }) {
  const idx = Number(e.detail.value)
  defectCode.value = defects.value[idx]?.defectCode || ''
}
</script>

<template>
  <view class="page">
    <view class="card">
      <view class="scan" @click="handleScan">{{ workOrderNo || t('mobile.scan.hint') }}</view>
      <view class="row">
        <button size="mini" @click="handleScan">{{ t('mobile.report.scan') }}</button>
        <button size="mini" type="primary" @click="loadWorkOrder">{{ t('mobile.report.load') }}</button>
      </view>
      <input v-model="workOrderNo" class="input" :placeholder="t('mobile.report.woPlaceholder')" />
    </view>

    <view v-if="detail" class="card">
      <view class="title">{{ detail.workOrderNo }} · {{ detail.materialName || detail.materialNo }}</view>
      <view class="meta">{{ t('mobile.report.planQty') }}: {{ detail.planQty }} · {{ detail.status }}</view>
      <view v-for="p in detail.progress" :key="p.processCode" class="progress">
        <text>{{ p.processCode }} {{ p.processName }}</text>
        <text>良 {{ p.goodQty }} / 可报 {{ p.remainQty }}</text>
      </view>
    </view>

    <view class="card">
      <view class="label">{{ t('mobile.report.process') }}</view>
      <picker :range="processOptions" range-key="processName" @change="onProcessPick">
        <view class="picker">
          {{ processCode || t('mobile.report.pickProcess') }}（{{ t('mobile.report.remain') }} {{ currentRemain }}）
        </view>
      </picker>

      <view v-if="showMachine" class="label">{{ t('mobile.report.machine') }}</view>
      <picker v-if="showMachine" :range="machines" range-key="machineName" @change="onMachinePick">
        <view class="picker">{{ machineNo || t('mobile.report.pickMachine') }}</view>
      </picker>

      <view class="qty-row">
        <view class="qty">
          <text>{{ t('mobile.report.good') }}</text>
          <input v-model.number="goodQty" type="number" class="qty-input" />
        </view>
        <view class="qty">
          <text>{{ t('mobile.report.defect') }}</text>
          <input v-model.number="defectQty" type="number" class="qty-input" />
        </view>
      </view>

      <view v-if="showDefect" class="label">{{ t('mobile.report.defectReason') }}</view>
      <picker v-if="showDefect" :range="defects" range-key="defectName" @change="onDefectPick">
        <view class="picker">{{ defectCode || t('mobile.report.pickDefect') }}</view>
      </picker>

      <button type="primary" :loading="submitting" @click="submitReport">{{ t('mobile.report.submit') }}</button>
    </view>

    <view v-for="(item, index) in recent" :key="index" class="card tiny">
      {{ item.processCode }} 良{{ item.goodQty }} 不良{{ item.defectQty }} · 剩余{{ item.remainQty }}
    </view>
  </view>
</template>

<style scoped lang="scss">
.page {
  padding: 24rpx;
  background: #f1f5f9;
  min-height: 100vh;
}
.card {
  background: #fff;
  border-radius: 16rpx;
  padding: 24rpx;
  margin-bottom: 20rpx;
}
.scan {
  min-height: 100rpx;
  background: #f8fafc;
  border-radius: 12rpx;
  padding: 24rpx;
  color: #64748b;
  margin-bottom: 16rpx;
}
.row {
  display: flex;
  gap: 16rpx;
  margin-bottom: 16rpx;
}
.input {
  background: #f8fafc;
  padding: 16rpx 20rpx;
  border-radius: 10rpx;
}
.title {
  font-weight: 600;
  margin-bottom: 8rpx;
}
.meta,
.progress {
  font-size: 24rpx;
  color: #64748b;
  display: flex;
  justify-content: space-between;
  margin-top: 8rpx;
}
.label {
  margin: 16rpx 0 8rpx;
  font-size: 26rpx;
  color: #334155;
}
.picker {
  background: #f8fafc;
  padding: 20rpx;
  border-radius: 10rpx;
}
.qty-row {
  display: flex;
  gap: 24rpx;
  margin: 20rpx 0;
}
.qty {
  flex: 1;
}
.qty-input {
  margin-top: 8rpx;
  background: #f8fafc;
  padding: 16rpx;
  border-radius: 10rpx;
}
.tiny {
  font-size: 24rpx;
  color: #475569;
}
button[type='primary'] {
  margin-top: 24rpx;
}
</style>
