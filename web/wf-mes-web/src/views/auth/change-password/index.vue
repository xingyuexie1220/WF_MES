<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { ElMessage } from 'element-plus'
import { Lock, View } from '@element-plus/icons-vue'
import { changePasswordApi } from '@/api/auth'
import { useUserStore } from '@/stores/auth/user'
import WfBrand from '@/components/WfBrand.vue'

const router = useRouter()
const userStore = useUserStore()
const { t } = useI18n()
const loading = ref(false)
const showPwd = ref({ old: false, new: false, confirm: false })

const form = reactive({
  oldPassword: '',
  newPassword: '',
  confirmPassword: ''
})

async function handleSubmit() {
  if (!form.oldPassword) {
    ElMessage.warning(t('changePassword.oldRequired'))
    return
  }
  if (!form.newPassword) {
    ElMessage.warning(t('changePassword.newRequired'))
    return
  }
  if (form.newPassword.length < 6) {
    ElMessage.warning(t('changePassword.tooShort'))
    return
  }
  if (form.newPassword !== form.confirmPassword) {
    ElMessage.warning(t('changePassword.mismatch'))
    return
  }
  if (form.oldPassword === form.newPassword) {
    ElMessage.warning(t('changePassword.sameAsOld'))
    return
  }

  loading.value = true
  try {
    const userInfo = await changePasswordApi({
      oldPassword: form.oldPassword,
      newPassword: form.newPassword
    })
    userStore.userInfo = userInfo
    ElMessage.success(t('changePassword.success'))
    router.replace('/dashboard')
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="change-password-page">
    <div class="change-password-page__panel">
      <div class="change-password-page__header">
        <WfBrand theme="dark" :size="32" />
        <h2 class="change-password-page__title">{{ t('changePassword.title') }}</h2>
        <p class="change-password-page__desc">{{ t('changePassword.desc') }}</p>
      </div>

      <form class="change-password-page__form" @submit.prevent="handleSubmit">
        <div class="change-password-page__field">
          <label>{{ t('changePassword.oldPassword') }}</label>
          <div class="change-password-page__input-wrap">
            <input
              v-model="form.oldPassword"
              :type="showPwd.old ? 'text' : 'password'"
              autocomplete="current-password"
              :placeholder="t('changePassword.oldPassword')"
            />
            <el-icon class="change-password-page__toggle" @click="showPwd.old = !showPwd.old"><View /></el-icon>
          </div>
        </div>

        <div class="change-password-page__field">
          <label>{{ t('changePassword.newPassword') }}</label>
          <div class="change-password-page__input-wrap">
            <input
              v-model="form.newPassword"
              :type="showPwd.new ? 'text' : 'password'"
              autocomplete="new-password"
              :placeholder="t('changePassword.newPassword')"
            />
            <el-icon class="change-password-page__toggle" @click="showPwd.new = !showPwd.new"><View /></el-icon>
          </div>
        </div>

        <div class="change-password-page__field">
          <label>{{ t('changePassword.confirmPassword') }}</label>
          <div class="change-password-page__input-wrap">
            <input
              v-model="form.confirmPassword"
              :type="showPwd.confirm ? 'text' : 'password'"
              autocomplete="new-password"
              :placeholder="t('changePassword.confirmPassword')"
            />
            <el-icon class="change-password-page__toggle" @click="showPwd.confirm = !showPwd.confirm"><View /></el-icon>
          </div>
        </div>

        <p class="change-password-page__hint">
          <el-icon><Lock /></el-icon>
          {{ t('changePassword.hint') }}
        </p>

        <el-button type="primary" size="large" class="change-password-page__submit" :loading="loading" @click="handleSubmit">
          {{ t('changePassword.submit') }}
        </el-button>
      </form>
    </div>
  </div>
</template>

<style scoped lang="scss">
.change-password-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #edf2ff 0%, #f8fafc 100%);
  padding: 24px;

  &__panel {
    width: 100%;
    max-width: 440px;
    padding: 32px 28px;
    background: #fff;
    border: 1px solid #eceff5;
    border-radius: 12px;
    box-shadow: 0 12px 28px rgba(0, 0, 0, 0.08);
  }

  &__header {
    margin-bottom: 24px;
  }

  &__title {
    margin: 16px 0 8px;
    font-size: 20px;
    font-weight: 600;
    color: var(--wf-text);
  }

  &__desc {
    margin: 0;
    font-size: 13px;
    line-height: 1.6;
    color: var(--wf-text-secondary);
  }

  &__field {
    margin-bottom: 16px;

    label {
      display: block;
      margin-bottom: 8px;
      font-size: 13px;
      color: var(--wf-text-secondary);
    }
  }

  &__input-wrap {
    display: flex;
    align-items: center;
    height: 44px;
    padding: 0 12px;
    border: 1px solid #ececec;
    border-radius: 6px;
    background: #fff;

    &:focus-within {
      border-color: #3a6cd1;
      box-shadow: 0 0 0 2px rgba(58, 108, 209, 0.15);
    }

    input {
      flex: 1;
      border: none;
      outline: none;
      font-size: 15px;
      background: transparent;
    }
  }

  &__toggle {
    color: #999;
    cursor: pointer;
  }

  &__hint {
    display: flex;
    align-items: center;
    gap: 6px;
    margin: 0 0 20px;
    font-size: 12px;
    color: var(--wf-text-muted);
  }

  &__submit {
    width: 100%;
  }
}
</style>
