<script setup lang="ts">
defineOptions({ name: 'Dashboard' })
import { computed, onMounted, onUnmounted, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import * as echarts from 'echarts'
import { useLayoutResize } from '@/composables/useLayoutResize'

const { t, locale } = useI18n()

const barPeriod = ref('month')
const tablePeriod = ref('month')

const summaryCards = computed(() => [
  { title: t('dashboard.stockInMonth'), value: '2,200', color: '#9254de', bg: '#f9f0ff' },
  { title: t('dashboard.stockOutMonth'), value: '1,400', color: '#1890ff', bg: '#e6f7ff' },
  { title: t('dashboard.stockInTotal'), value: '28,000', color: '#52c41a', bg: '#f6ffed' },
  { title: t('dashboard.stockOutTotal'), value: '14,000', color: '#f5222d', bg: '#fff1f0' }
])

const periodOptions = computed(() => [
  { label: t('period.month'), value: 'month' },
  { label: t('period.lastMonth'), value: 'lastMonth' },
  { label: t('period.threeMonth'), value: 'threeMonth' },
  { label: t('period.halfYear'), value: 'halfYear' }
])

const tableData = computed(() => [
  { dept: '公共事业部', date: '2026-07-01', income: 12000, expense: 8600, consume: 3200, balance: 3400, remark: t('common.placeholder') },
  { dept: '公共事业部', date: '2026-07-02', income: 9800, expense: 7200, consume: 2800, balance: 2600, remark: t('common.placeholder') },
  { dept: '机加车间', date: '2026-07-01', income: 15600, expense: 11200, consume: 4100, balance: 4400, remark: t('common.placeholder') },
  { dept: '机加车间', date: '2026-07-02', income: 14300, expense: 10800, consume: 3900, balance: 3500, remark: t('common.placeholder') },
  { dept: '白班一组', date: '2026-07-01', income: 8200, expense: 6100, consume: 2100, balance: 2100, remark: t('common.placeholder') }
])

const messages = computed(() => [
  { title: t('dashboard.msgSystem'), content: t('dashboard.msgSystemContent'), time: '08:30' },
  { title: t('dashboard.msgProduction'), content: t('dashboard.msgProductionContent'), time: '09:15' },
  { title: t('dashboard.msgDevice'), content: t('dashboard.msgDeviceContent'), time: '10:02' }
])

const barChartRef = ref<HTMLDivElement>()
const pieChartRef = ref<HTMLDivElement>()
let barChart: echarts.ECharts | null = null
let pieChart: echarts.ECharts | null = null

function buildBarOption() {
  const dates = ['2026-06-23', '2026-06-24', '2026-06-25', '2026-06-26', '2026-06-27', '2026-06-28', '2026-06-29', '2026-06-30', '2026-07-01', '2026-07-02']
  return {
    tooltip: { trigger: 'axis' },
    legend: { data: [t('dashboard.stockIn'), t('dashboard.stockOut')], right: 0 },
    grid: { left: 40, right: 20, top: 40, bottom: 30 },
    xAxis: { type: 'category', data: dates, axisLabel: { rotate: 30 } },
    yAxis: { type: 'value' },
    series: [
      { name: t('dashboard.stockIn'), type: 'bar', barMaxWidth: 18, itemStyle: { color: '#69c0ff' }, data: [120, 132, 101, 134, 90, 230, 210, 182, 191, 220] },
      { name: t('dashboard.stockOut'), type: 'bar', barMaxWidth: 18, itemStyle: { color: '#1890ff' }, data: [80, 92, 71, 94, 60, 130, 110, 102, 121, 140] }
    ]
  }
}

function buildPieOption() {
  return {
    tooltip: { trigger: 'item' },
    legend: { bottom: 0, left: 'center' },
    series: [
      {
        type: 'pie',
        radius: ['42%', '68%'],
        center: ['50%', '45%'],
        label: { show: false },
        data: [
          { value: 1048, name: t('dashboard.produced'), itemStyle: { color: '#1890ff' } },
          { value: 735, name: t('dashboard.pending'), itemStyle: { color: '#52c41a' } },
          { value: 580, name: t('dashboard.wip'), itemStyle: { color: '#faad14' } },
          { value: 484, name: t('dashboard.scrap'), itemStyle: { color: '#f5222d' } },
          { value: 300, name: t('dashboard.rework'), itemStyle: { color: '#9254de' } }
        ]
      }
    ]
  }
}

function refreshCharts() {
  barChart?.setOption(buildBarOption(), true)
  pieChart?.setOption(buildPieOption(), true)
}

function resizeCharts() {
  barChart?.resize()
  pieChart?.resize()
}

onMounted(() => {
  if (barChartRef.value) {
    barChart = echarts.init(barChartRef.value)
    barChart.setOption(buildBarOption())
  }
  if (pieChartRef.value) {
    pieChart = echarts.init(pieChartRef.value)
    pieChart.setOption(buildPieOption())
  }
  window.addEventListener('resize', resizeCharts)
})

useLayoutResize(resizeCharts)

watch(locale, () => {
  refreshCharts()
})

onUnmounted(() => {
  window.removeEventListener('resize', resizeCharts)
  barChart?.dispose()
  pieChart?.dispose()
})
</script>

<template>
  <div class="wf-dashboard">
    <h2 class="wf-dashboard__title">{{ t('route.dashboard') }}</h2>
    <div class="wf-dashboard__body">
      <div class="wf-dashboard-main">
      <el-row :gutter="12" class="wf-stat-row">
        <el-col v-for="card in summaryCards" :key="card.title" :xs="24" :sm="12" :md="6">
          <div class="wf-stat-card">
            <div class="wf-stat-info">
              <div class="wf-stat-title">{{ card.title }}</div>
              <div class="wf-stat-value">{{ card.value }}</div>
            </div>
            <div class="wf-stat-icon" :style="{ background: card.bg, color: card.color }">
              <span>◆</span>
            </div>
          </div>
        </el-col>
      </el-row>

      <el-card shadow="never" class="wf-panel">
        <template #header>
          <div class="wf-panel-header">
            <span>{{ t('dashboard.inOutChart') }}</span>
            <el-radio-group v-model="barPeriod" size="small">
              <el-radio-button v-for="item in periodOptions" :key="item.value" :value="item.value">
                {{ item.label }}
              </el-radio-button>
            </el-radio-group>
          </div>
        </template>
        <div ref="barChartRef" class="wf-chart wf-chart-bar" />
      </el-card>

      <el-card shadow="never" class="wf-panel">
        <template #header>
          <div class="wf-panel-header">
            <span>{{ t('dashboard.deptExpense') }}</span>
            <el-radio-group v-model="tablePeriod" size="small">
              <el-radio-button v-for="item in periodOptions" :key="`t-${item.value}`" :value="item.value">
                {{ item.label }}
              </el-radio-button>
            </el-radio-group>
          </div>
        </template>
        <vxe-table :data="tableData" border stripe size="small">
          <vxe-column type="seq" title="#" width="50" />
          <vxe-column field="dept" :title="t('dashboard.dept')" min-width="120" />
          <vxe-column field="date" :title="t('dashboard.date')" width="120" />
          <vxe-column field="income" :title="t('dashboard.income')" width="100" />
          <vxe-column field="expense" :title="t('dashboard.expense')" width="100" />
          <vxe-column field="consume" :title="t('dashboard.consume')" width="100" />
          <vxe-column field="balance" :title="t('dashboard.balance')" width="100" />
          <vxe-column field="remark" :title="t('dashboard.remark')" min-width="120" />
        </vxe-table>
      </el-card>
      </div>

    <div class="wf-dashboard-side">
      <el-card shadow="never" class="wf-panel">
        <template #header>{{ t('dashboard.productionStats') }}</template>
        <div ref="pieChartRef" class="wf-chart wf-chart-pie" />
      </el-card>

      <el-card shadow="never" class="wf-panel wf-message-card">
        <template #header>{{ t('dashboard.siteMessage') }}</template>
        <div class="wf-message-list">
          <div v-for="(item, index) in messages" :key="index" class="wf-message-item">
            <div class="wf-message-title">
              <span>{{ item.title }}</span>
              <span class="wf-message-time">{{ item.time }}</span>
            </div>
            <div class="wf-message-content">{{ item.content }}</div>
          </div>
        </div>
      </el-card>
    </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.wf-dashboard {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.wf-dashboard__title {
  margin: 0;
  font-size: 18px;
  font-weight: 600;
  color: var(--wf-text);
}

.wf-dashboard__body {
  display: flex;
  gap: 12px;
  align-items: flex-start;
}

.wf-dashboard-main {
  flex: 1;
  min-width: 0;
}

.wf-dashboard-side {
  width: 320px;
  flex-shrink: 0;
}

.wf-stat-row {
  margin-bottom: 12px;
}

.wf-stat-card {
  background: #fff;
  border-radius: 4px;
  padding: 16px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.03);
}

.wf-stat-title {
  color: #8c8c8c;
  font-size: 13px;
}

.wf-stat-value {
  margin-top: 8px;
  font-size: 28px;
  font-weight: 600;
  color: #262626;
}

.wf-stat-icon {
  width: 48px;
  height: 48px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 22px;
}

.wf-panel {
  margin-bottom: 12px;
  border: none;
}

.wf-panel-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.wf-chart-bar {
  height: 320px;
}

.wf-chart-pie {
  height: 280px;
}

.wf-message-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.wf-message-item {
  padding-bottom: 12px;
  border-bottom: 1px dashed #f0f0f0;

  &:last-child {
    border-bottom: none;
    padding-bottom: 0;
  }
}

.wf-message-title {
  display: flex;
  justify-content: space-between;
  font-size: 13px;
  color: #262626;
  margin-bottom: 4px;
}

.wf-message-time {
  color: #bfbfbf;
  font-size: 12px;
}

.wf-message-content {
  color: #8c8c8c;
  font-size: 12px;
  line-height: 1.6;
}

@media (max-width: 1200px) {
  .wf-dashboard__body {
    flex-direction: column;
  }

  .wf-dashboard-side {
    width: 100%;
  }
}
</style>
