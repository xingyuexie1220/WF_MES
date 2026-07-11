<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { appConfig } from '@/config/app'
import { useLocaleStore } from '@/stores/locale'
import { useUserStore } from '@/stores/user'
import WfLocalePicker from '@/components/WfLocalePicker.vue'
import WfFactoryPicker from '@/components/WfFactoryPicker.vue'
import type { FactorySummary } from '@/types/auth'
import { isLoggedIn } from '@/utils/auth'

const { t } = useI18n()
const localeStore = useLocaleStore()
const userStore = useUserStore()

const userName = ref('')
const password = ref('')
const loading = ref(false)
const showFactoryPicker = ref(false)
const factories = ref<FactorySummary[]>([])

onMounted(() => {
  localeStore.applyLocale(localeStore.locale)
  if (isLoggedIn()) {
    uni.switchTab({ url: '/pages/home/index' })
  }
})

async function afterLogin() {
  uni.showToast({ title: t('login.success'), icon: 'success' })
  if (userStore.userInfo?.mustChangePassword) {
    uni.reLaunch({ url: '/pages/change-password/index' })
  } else {
    uni.switchTab({ url: '/pages/home/index' })
  }
}

async function handleLogin() {
  if (!userName.value || !password.value) {
    uni.showToast({ title: t('login.username'), icon: 'none' })
    return
  }

  loading.value = true
  try {
    const data = await userStore.login(userName.value, password.value)
    if (data.needSelectFactory && data.factories?.length) {
      factories.value = data.factories
      showFactoryPicker.value = true
      return
    }
    await afterLogin()
  } finally {
    loading.value = false
  }
}

async function handleSelectFactory(factory: FactorySummary) {
  loading.value = true
  try {
    await userStore.selectFactory(userName.value, password.value, factory)
    showFactoryPicker.value = false
    await afterLogin()
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <view class="page">
    <view class="top-bar">
      <WfLocalePicker :show-label="false" />
    </view>

    <view class="hero">
      <view class="logo">{{ t('common.brand').slice(0, 3) }}</view>
      <text class="title">{{ appConfig.appName }}</text>
      <text class="subtitle">{{ t('login.subtitle') }}</text>
    </view>

    <view class="form-card">
      <view class="field">
        <text class="label">{{ t('login.usernameLabel') }}</text>
        <input v-model="userName" class="input" :placeholder="t('login.username')" />
      </view>
      <view class="field">
        <text class="label">{{ t('login.passwordLabel') }}</text>
        <input v-model="password" class="input" password :placeholder="t('login.password')" />
      </view>

      <button class="btn-primary" :loading="loading" @click="handleLogin">
        {{ t('login.submit') }}
      </button>
    </view>

    <WfFactoryPicker
      :show="showFactoryPicker"
      :factories="factories"
      @close="showFactoryPicker = false"
      @select="handleSelectFactory"
    />
  </view>
</template>

<style scoped lang="scss">
.page {
  min-height: 100vh;
  background: linear-gradient(165deg, #1e3a8a 0%, #2563eb 45%, #eff6ff 45%);
  padding: 48rpx 40rpx 40rpx;
  box-sizing: border-box;
}

.top-bar {
  display: flex;
  justify-content: flex-end;
  margin-bottom: 24rpx;
}

.hero {
  color: #fff;
  margin-bottom: 48rpx;
}

.logo {
  width: 120rpx;
  height: 120rpx;
  border-radius: 28rpx;
  background: rgba(255, 255, 255, 0.18);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 40rpx;
  font-weight: 700;
  margin-bottom: 24rpx;
}

.title {
  display: block;
  font-size: 44rpx;
  font-weight: 700;
}

.subtitle {
  display: block;
  margin-top: 12rpx;
  font-size: 26rpx;
  opacity: 0.85;
}

.form-card {
  background: #fff;
  border-radius: 24rpx;
  padding: 48rpx 36rpx;
  box-shadow: 0 16rpx 48rpx rgba(15, 35, 80, 0.12);
}

.field {
  margin-bottom: 28rpx;
}

.label {
  display: block;
  font-size: 26rpx;
  color: #64748b;
  margin-bottom: 12rpx;
}

.input {
  height: 88rpx;
  padding: 0 24rpx;
  background: #f8fafc;
  border-radius: 16rpx;
  font-size: 30rpx;
}

.btn-primary {
  margin-top: 16rpx;
  height: 88rpx;
  line-height: 88rpx;
  background: #2563eb;
  color: #fff;
  border-radius: 16rpx;
  font-size: 32rpx;
  border: none;
}
</style>
