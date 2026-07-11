<script setup lang="ts">
import { ref } from 'vue'
import { onShow } from '@dcloudio/uni-app'
import { useI18n } from 'vue-i18n'
import { useUserStore } from '@/stores/user'

const { t } = useI18n()
const userStore = useUserStore()

const oldPassword = ref('')
const newPassword = ref('')
const confirmPassword = ref('')
const loading = ref(false)

onShow(() => {
  uni.setNavigationBarTitle({ title: t('mobile.password.title') })
})

async function handleSubmit() {
  if (!oldPassword.value) {
    uni.showToast({ title: t('mobile.password.oldRequired'), icon: 'none' })
    return
  }
  if (!newPassword.value) {
    uni.showToast({ title: t('mobile.password.newRequired'), icon: 'none' })
    return
  }
  if (newPassword.value.length < 6) {
    uni.showToast({ title: t('mobile.password.tooShort'), icon: 'none' })
    return
  }
  if (newPassword.value !== confirmPassword.value) {
    uni.showToast({ title: t('mobile.password.mismatch'), icon: 'none' })
    return
  }
  if (oldPassword.value === newPassword.value) {
    uni.showToast({ title: t('mobile.password.sameAsOld'), icon: 'none' })
    return
  }

  loading.value = true
  try {
    await userStore.changePassword(oldPassword.value, newPassword.value)
    uni.showToast({ title: t('mobile.password.success'), icon: 'success' })
    uni.switchTab({ url: '/pages/home/index' })
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <view class="change-password-page">
    <view class="title">{{ t('mobile.password.title') }}</view>
    <view class="desc">{{ t('mobile.password.desc') }}</view>
    <u-form>
      <u-form-item :label="t('mobile.password.old')">
        <u-input v-model="oldPassword" type="password" :placeholder="t('mobile.password.oldPlaceholder')" />
      </u-form-item>
      <u-form-item :label="t('mobile.password.new')">
        <u-input v-model="newPassword" type="password" :placeholder="t('mobile.password.newPlaceholder')" />
      </u-form-item>
      <u-form-item :label="t('mobile.password.confirm')">
        <u-input v-model="confirmPassword" type="password" :placeholder="t('mobile.password.confirmPlaceholder')" />
      </u-form-item>
    </u-form>
    <u-button type="primary" :loading="loading" :text="t('mobile.password.submit')" @click="handleSubmit" />
  </view>
</template>

<style scoped lang="scss">
.change-password-page {
  padding: 80rpx 40rpx;
}

.title {
  font-size: 44rpx;
  font-weight: 700;
  text-align: center;
}

.desc {
  margin: 16rpx 0 48rpx;
  text-align: center;
  color: #909399;
  line-height: 1.6;
}
</style>
