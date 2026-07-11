<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { ArrowDown, Bell, Expand, Fold, FullScreen } from '@element-plus/icons-vue'
import { useUserStore } from '@/stores/auth/user'
import { useMenuStore } from '@/stores/layout/menu'
import { useNoticeStore } from '@/stores/notice/notice'
import WfLocaleSwitch from '@/components/WfLocaleSwitch.vue'
import WfFactorySwitch from '@/components/WfFactorySwitch.vue'
import WfBreadcrumb from '@/layout/WfBreadcrumb.vue'
import WfMessageDrawer from '@/layout/WfMessageDrawer.vue'

const userStore = useUserStore()
const menuStore = useMenuStore()
const noticeStore = useNoticeStore()
const { t } = useI18n()

const nowText = ref('')
const isFullscreen = ref(false)
const messageVisible = ref(false)
let timer: number | undefined

const displayName = computed(
  () => userStore.userInfo?.nickName || userStore.userInfo?.userName || t('layout.user')
)

const avatarText = computed(() => displayName.value.slice(0, 1).toUpperCase())

function updateClock() {
  const now = new Date()
  const pad = (n: number) => String(n).padStart(2, '0')
  nowText.value = `${now.getFullYear()}-${pad(now.getMonth() + 1)}-${pad(now.getDate())} ${pad(now.getHours())}:${pad(now.getMinutes())}:${pad(now.getSeconds())}`
}

function handleFullscreen() {
  if (!document.fullscreenElement) {
    document.documentElement.requestFullscreen?.()
  } else {
    document.exitFullscreen?.()
  }
}

function handleMessages() {
  messageVisible.value = true
}

async function handleLogout() {
  await userStore.logout()
}

function syncFullscreenState() {
  isFullscreen.value = Boolean(document.fullscreenElement)
}

onMounted(() => {
  updateClock()
  timer = window.setInterval(updateClock, 1000)
  document.addEventListener('fullscreenchange', syncFullscreenState)
})

onUnmounted(() => {
  if (timer) {
    window.clearInterval(timer)
  }
  document.removeEventListener('fullscreenchange', syncFullscreenState)
})
</script>

<template>
  <header class="wf-header">
    <div class="wf-header__left">
      <el-button class="wf-header__icon-btn" link @click="menuStore.toggleCollapsed()">
        <el-icon><component :is="menuStore.collapsed ? Expand : Fold" /></el-icon>
      </el-button>
      <WfBreadcrumb />
    </div>

    <div class="wf-header__right">
      <div class="wf-header__message" @click="handleMessages">
        <el-badge :value="noticeStore.unreadCount" :hidden="!noticeStore.unreadCount" :max="99">
          <el-icon><Bell /></el-icon>
        </el-badge>
      </div>

      <WfFactorySwitch class="wf-header__factory" />
      <WfLocaleSwitch class="wf-header__locale" />

      <div class="wf-header__fullscreen" @click="handleFullscreen">
        <el-icon><FullScreen /></el-icon>
      </div>

      <el-dropdown trigger="hover" class="wf-header__user">
        <div class="wf-header__user-trigger">
          <div class="wf-header__avatar wf-header__avatar--fallback">{{ avatarText }}</div>
          <div class="wf-header__user-meta">
            <div class="wf-header__user-name">
              {{ displayName }}
              <el-icon class="wf-header__user-arrow"><ArrowDown /></el-icon>
            </div>
            <div class="wf-header__clock">{{ nowText }}</div>
          </div>
        </div>
        <template #dropdown>
          <el-dropdown-menu>
            <el-dropdown-item disabled>{{ displayName }}</el-dropdown-item>
            <el-dropdown-item divided @click="handleLogout">{{ t('layout.logout') }}</el-dropdown-item>
          </el-dropdown-menu>
        </template>
      </el-dropdown>
    </div>
  </header>

  <WfMessageDrawer v-model="messageVisible" />
</template>

<style scoped lang="scss">
.wf-header {
  height: var(--wf-header-height);
  background: var(--wf-header-blue);
  border-bottom: 1px solid var(--wf-border);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 12px 0 4px;
  flex-shrink: 0;
  box-sizing: border-box;

  &__left,
  &__right {
    display: flex;
    align-items: center;
  }

  &__left {
    flex: 1;
    min-width: 0;
    gap: 8px;
  }

  &__right {
    gap: 8px;
  }

  &__icon-btn,
  &__message,
  &__fullscreen {
    color: #fff !important;
    cursor: pointer;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 36px;
    height: 36px;
    border-radius: 4px;
    transition: background-color 0.2s ease;

    &:hover {
      background: rgba(255, 255, 255, 0.12);
    }

    .el-icon {
      font-size: 18px;
    }
  }

  &__locale,
  &__factory {
    color: #fff;

    :deep(.wf-locale-switch) {
      color: #fff;
    }
  }

  &__user-trigger {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 4px 10px;
    border-radius: 4px;
    cursor: pointer;
    outline: none;

    &:hover {
      background: rgba(255, 255, 255, 0.12);
    }
  }

  &__avatar {
    width: 34px;
    height: 34px;
    border-radius: 50%;
    border: 1px solid rgba(255, 255, 255, 0.65);
    object-fit: cover;
    background: #fff;

    &--fallback {
      display: flex;
      align-items: center;
      justify-content: center;
      color: var(--wf-header-blue);
      font-weight: 700;
      font-size: 14px;
    }
  }

  &__user-meta {
    display: flex;
    flex-direction: column;
    justify-content: center;
    min-width: 0;
  }

  &__user-name {
    display: flex;
    align-items: center;
    gap: 4px;
    color: #fff;
    font-size: 13px;
    font-weight: 700;
    line-height: 1.2;
  }

  &__user-arrow {
    font-size: 12px;
  }

  &__clock {
    margin-top: 2px;
    color: rgba(255, 255, 255, 0.92);
    font-size: 12px;
    line-height: 1.2;
  }
}
</style>
