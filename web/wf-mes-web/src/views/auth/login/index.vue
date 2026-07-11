<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { ElMessage } from 'element-plus'
import { Lock, User, View } from '@element-plus/icons-vue'
import { useUserStore } from '@/stores/auth/user'
import WfLocaleSwitch from '@/components/WfLocaleSwitch.vue'
import WfBrand from '@/components/WfBrand.vue'
import loginBg from '@/assets/images/login-bg.png'
import type { FactorySummary } from '@/types/common/factory'
import type { LoginResponse } from '@/types/auth/login'

const router = useRouter()
const userStore = useUserStore()
const { t } = useI18n()
const loading = ref(false)
const showPwd = ref(false)
const year = new Date().getFullYear()
const factoryDialogVisible = ref(false)
const pendingFactories = ref<FactorySummary[]>([])

const form = reactive({
  userName: '',
  password: ''
})

const submitText = computed(() => (loading.value ? `${t('login.loggingIn')}...` : t('login.submit')))

async function afterLoginSuccess() {
  ElMessage.success(t('login.success'))
  if (userStore.userInfo?.mustChangePassword) {
    router.push('/change-password')
  } else {
    router.push('/dashboard')
  }
}

async function handleLogin() {
  if (loading.value) {
    return
  }
  if (!form.userName.trim()) {
    ElMessage.warning(t('login.usernameRequired'))
    return
  }
  if (!form.password) {
    ElMessage.warning(t('login.passwordRequired'))
    return
  }

  loading.value = true
  try {
    const result = (await userStore.login(form)) as LoginResponse
    if (result.needSelectFactory && result.factories?.length) {
      pendingFactories.value = result.factories
      factoryDialogVisible.value = true
      return
    }
    await afterLoginSuccess()
  } finally {
    loading.value = false
  }
}

async function handleSelectFactory(factory: FactorySummary) {
  loading.value = true
  try {
    await userStore.selectFactory({
      userName: form.userName,
      password: form.password,
      factoryId: factory.id
    })
    factoryDialogVisible.value = false
    await afterLoginSuccess()
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="login-page">
    <div class="login-page__bg" :style="{ backgroundImage: `url(${loginBg})` }" aria-hidden="true" />
    <div class="login-page__project-name">
      <WfBrand />
    </div>

    <div class="login-page__panel">
      <div class="login-page__lang">
        <WfLocaleSwitch />
      </div>

      <div v-loading="loading" class="login-form-container" :element-loading-text="`${t('login.loggingIn')}...`">
      <div class="login-form__header">
        <WfBrand theme="dark" :size="32" />
        <div class="login-form__title-line" />
        <p class="login-form__subtitle">{{ t('login.welcome') }}</p>
      </div>

      <form class="login-form__content" @submit.prevent="handleLogin" @keyup.enter="handleLogin">
        <div class="login-form__input-group">
          <div class="login-form__input-icon">
            <el-icon><User /></el-icon>
          </div>
          <input
            v-model.trim="form.userName"
            type="text"
            autocomplete="username"
            class="login-form__input"
            :placeholder="t('login.username')"
            autofocus
          />
        </div>

        <div class="login-form__input-group">
          <div class="login-form__input-icon">
            <el-icon><Lock /></el-icon>
          </div>
          <input
            v-model.trim="form.password"
            :type="showPwd ? 'text' : 'password'"
            autocomplete="current-password"
            class="login-form__input"
            :placeholder="t('login.password')"
          />
          <div
            class="login-form__pwd-toggle"
            :class="{ active: showPwd }"
            @click="showPwd = !showPwd"
          >
            <el-icon><View /></el-icon>
          </div>
        </div>
      </form>

      <div class="login-form__submit">
        <el-button type="primary" size="large" :loading="loading" @click="handleLogin">
          {{ submitText }}
        </el-button>
      </div>
      </div>
    </div>

    <footer class="login-page__footer">© {{ year }} {{ t('common.brand') }}</footer>

    <el-dialog v-model="factoryDialogVisible" :title="t('factory.selectTitle')" width="420px" :close-on-click-modal="false">
      <p class="login-factory-tip">{{ t('factory.selectTip') }}</p>
      <div class="login-factory-list">
        <el-button
          v-for="item in pendingFactories"
          :key="item.id"
          class="login-factory-item"
          @click="handleSelectFactory(item)"
        >
          {{ item.factoryName }}
        </el-button>
      </div>
    </el-dialog>
  </div>
</template>

<style scoped lang="scss">
$primary-color: #3a6cd1;
$text-color: #323233;
$text-color-light: #7d7c7c;
$border-color: #ececec;
$input-height: 45px;

.login-page {
  position: relative;
  width: 100%;
  height: 100vh;
  background: rgb(246, 247, 252);
  min-width: 1200px;
  z-index: 0;
  overflow: hidden;

  &__bg {
    position: absolute;
    left: 0;
    top: 0;
    width: 50%;
    height: 100%;
    z-index: 0;
    background-size: cover;
    background-position: center;
    background-repeat: no-repeat;
  }

  &__project-name {
    position: absolute;
    top: 40px;
    left: 40px;
    z-index: 1;
  }

  &__panel {
    position: relative;
    width: 50%;
    height: 100%;
    margin-left: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 1;
  }

  &__lang {
    position: absolute;
    top: 40px;
    right: 40px;
    z-index: 2;
  }

  &__footer {
    position: absolute;
    bottom: 0.5rem;
    width: 100%;
    text-align: center;
    font-size: 12px;
    color: #4f4f4f;
    padding-bottom: 10px;
    z-index: 1;
  }
}

.login-form-container {
  width: 420px;
  min-height: 280px;
  padding: 32px 28px;
  background: #fff;
  border: 1px solid #eceff5;
  box-shadow: 0 12px 28px rgba(0, 0, 0, 0.08);
  border-radius: 12px;
  z-index: 1;
}

.login-form {
  &__header {
    margin-bottom: 30px;
  }

  &__title-line {
    height: 3px;
    width: 100%;
    background-image: linear-gradient(to right, #6598ff, transparent);
  }

  &__subtitle {
    font-size: 13px;
    color: $text-color-light;
    margin: 10px 0 0;
  }

  &__input-group {
    display: flex;
    align-items: center;
    height: $input-height;
    padding-left: 20px;
    margin-bottom: 20px;
    border: 1px solid $border-color;
    border-radius: 5px;
    background: #fff;
    position: relative;
    transition: all 0.2s ease;

    &:focus-within {
      border-color: $primary-color;
      box-shadow: 0 0 0 2px rgba(58, 108, 209, 0.2);
    }
  }

  &__input-icon {
    color: #7a7a7a;
    padding-right: 20px;
    display: flex;
    align-items: center;

    .el-icon {
      font-size: 18px;
    }
  }

  &__input {
    flex: 1;
    height: 100%;
    border: none;
    outline: none;
    font-size: 16px;
    color: $text-color;
    padding-right: 40px;
    background: transparent;

    &:-webkit-autofill {
      box-shadow: 0 0 0 1000px white inset;
      -webkit-box-shadow: 0 0 0 1000px white inset;
    }
  }

  &__pwd-toggle {
    position: absolute;
    right: 20px;
    top: 50%;
    transform: translateY(-50%);
    color: #999;
    cursor: pointer;
    width: 20px;
    height: 20px;
    display: flex;
    align-items: center;
    justify-content: center;
    transition: color 0.2s ease;

    .el-icon {
      font-size: 18px;
    }

    &:hover,
    &.active {
      color: $primary-color;
    }
  }

  &__submit {
    box-shadow: 2px 4px 11px #a4c2ff;
    margin-top: 16px;

    :deep(.el-button) {
      width: 100%;
      padding: 12px 0;
      font-size: 14px !important;
      border-radius: 5px;
      transition: all 0.2s ease;
      border: none;
      background-color: $primary-color;

      &:not(:disabled):hover {
        background-color: #2d5bb8;
      }
    }
  }
}

@media screen and (max-width: 1200px) {
  .login-page {
    min-width: auto;
  }
}

@media screen and (max-width: 700px) {
  .login-page {
    min-width: auto;

    &__bg,
    &__project-name {
      display: none;
    }

    &__panel {
      width: 100%;
      margin-left: 0;
      padding: 2rem;
    }

    &__lang {
      top: 20px;
      right: 20px;
    }
  }

  .login-form-container {
    width: 100%;
    max-width: 400px;
  }
}

.login-factory-tip {
  margin: 0 0 16px;
  color: #606266;
  font-size: 14px;
}

.login-factory-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.login-factory-item {
  width: 100%;
  justify-content: flex-start;
}
</style>
