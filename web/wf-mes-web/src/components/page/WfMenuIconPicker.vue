<script setup lang="ts">
import * as ElementPlusIconsVue from '@element-plus/icons-vue'
import { computed, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { Search } from '@element-plus/icons-vue'
import { resolveMenuIcon } from '@/utils/menu'

const model = defineModel<string>({ default: '' })

const { t } = useI18n()
const popoverVisible = ref(false)
const keyword = ref('')

const iconNames = Object.keys(ElementPlusIconsVue).sort()

const filteredIcons = computed(() => {
  const normalized = keyword.value.trim().toLowerCase()
  if (!normalized) {
    return iconNames
  }
  return iconNames.filter((name) => name.toLowerCase().includes(normalized))
})

function selectIcon(name: string) {
  model.value = name
  popoverVisible.value = false
  keyword.value = ''
}

function clearIcon() {
  model.value = ''
}
</script>

<template>
  <div class="wf-menu-icon-picker">
    <el-popover
      v-model:visible="popoverVisible"
      placement="bottom-start"
      :width="360"
      trigger="click"
      popper-class="wf-menu-icon-picker__popover"
    >
      <template #reference>
        <div class="wf-menu-icon-picker__trigger">
          <el-input
            :model-value="model"
            readonly
            :placeholder="t('system.menu.iconPickerPlaceholder')"
          >
            <template #prefix>
              <el-icon class="wf-menu-icon-picker__preview">
                <component :is="resolveMenuIcon(model)" />
              </el-icon>
            </template>
            <template #suffix>
              <el-button v-if="model" link type="primary" @click.stop="clearIcon">
                {{ t('system.menu.iconClear') }}
              </el-button>
            </template>
          </el-input>
        </div>
      </template>

      <div class="wf-menu-icon-picker__panel">
        <el-input
          v-model="keyword"
          clearable
          :prefix-icon="Search"
          :placeholder="t('system.menu.iconPickerSearch')"
          class="wf-menu-icon-picker__search"
        />
        <el-scrollbar max-height="280px">
          <div v-if="filteredIcons.length" class="wf-menu-icon-picker__grid">
            <button
              v-for="name in filteredIcons"
              :key="name"
              type="button"
              class="wf-menu-icon-picker__item"
              :class="{ 'is-active': model === name }"
              :title="name"
              @click="selectIcon(name)"
            >
              <el-icon><component :is="resolveMenuIcon(name)" /></el-icon>
              <span>{{ name }}</span>
            </button>
          </div>
          <el-empty v-else :description="t('system.menu.iconEmpty')" :image-size="64" />
        </el-scrollbar>
      </div>
    </el-popover>
  </div>
</template>

<style scoped lang="scss">
.wf-menu-icon-picker {
  width: 100%;

  &__trigger {
    width: 100%;
  }

  &__preview {
    font-size: 16px;
    color: var(--wf-primary);
  }

  &__search {
    margin-bottom: 10px;
  }

  &__grid {
    display: grid;
    grid-template-columns: repeat(4, minmax(0, 1fr));
    gap: 8px;
  }

  &__item {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 4px;
    padding: 8px 4px;
    border: 1px solid #ebeef5;
    border-radius: 6px;
    background: #fff;
    cursor: pointer;
    transition: border-color 0.15s, background 0.15s;

    .el-icon {
      font-size: 18px;
      color: var(--wf-text);
    }

    span {
      width: 100%;
      font-size: 10px;
      line-height: 1.2;
      color: var(--wf-text-muted);
      text-align: center;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    &:hover,
    &.is-active {
      border-color: var(--wf-primary);
      background: rgba(45, 140, 240, 0.06);
    }

    &.is-active .el-icon {
      color: var(--wf-primary);
    }
  }
}
</style>
