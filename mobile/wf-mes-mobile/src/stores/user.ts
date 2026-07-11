import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import {
  changePasswordApi,
  getMobileMenusApi,
  getUserInfoApi,
  loginApi,
  logoutApi,
  selectFactoryApi,
  switchFactoryApi
} from '@/api/auth'
import type { FactorySummary, LoginResult, UserInfo } from '@/types/auth'
import {
  clearAuthStorage,
  getStoredUserInfo,
  isLoggedIn,
  mustChangePassword,
  setTokens,
  setUserInfo
} from '@/utils/auth'

export const useUserStore = defineStore('wf-user', () => {
  const userInfo = ref<UserInfo | null>(getStoredUserInfo())
  const mobileMenus = ref<Array<Record<string, unknown>>>([])

  const displayName = computed(() => userInfo.value?.nickName || userInfo.value?.userName || '')
  const currentFactory = computed(() => ({
    id: userInfo.value?.factoryId,
    code: userInfo.value?.factoryCode,
    name: userInfo.value?.factoryName
  }))
  const accessibleFactories = computed(() => userInfo.value?.accessibleFactories ?? [])

  function applyLoginResult(data: LoginResult) {
    setTokens(data.accessToken, data.refreshToken, data.userInfo.factoryId)
    setUserInfo(data.userInfo)
    userInfo.value = data.userInfo
  }

  async function login(userName: string, password: string, factoryId?: number) {
    const data = await loginApi(userName, password, factoryId)
    if (data.needSelectFactory) {
      return data
    }
    applyLoginResult(data)
    return data
  }

  async function selectFactory(userName: string, password: string, factory: FactorySummary) {
    const data = await selectFactoryApi(userName, password, factory.id)
    applyLoginResult(data)
    return data
  }

  async function switchFactory(factoryId: number) {
    const data = await switchFactoryApi(factoryId)
    applyLoginResult(data)
    mobileMenus.value = []
    return data
  }

  async function fetchUserInfo() {
    userInfo.value = await getUserInfoApi()
    setUserInfo(userInfo.value)
  }

  async function fetchMobileMenus() {
    mobileMenus.value = await getMobileMenusApi()
    return mobileMenus.value
  }

  async function changePassword(oldPassword: string, newPassword: string) {
    userInfo.value = await changePasswordApi(oldPassword, newPassword)
    setUserInfo(userInfo.value)
  }

  async function logout() {
    try {
      await logoutApi()
    } finally {
      reset()
      clearAuthStorage()
      uni.reLaunch({ url: '/pages/login/index' })
    }
  }

  function reset() {
    userInfo.value = null
    mobileMenus.value = []
  }

  function checkAuthGuard() {
    if (!isLoggedIn()) {
      uni.reLaunch({ url: '/pages/login/index' })
      return false
    }
    if (mustChangePassword()) {
      uni.reLaunch({ url: '/pages/change-password/index' })
      return false
    }
    return true
  }

  return {
    userInfo,
    mobileMenus,
    displayName,
    currentFactory,
    accessibleFactories,
    login,
    selectFactory,
    switchFactory,
    fetchUserInfo,
    fetchMobileMenus,
    changePassword,
    logout,
    reset,
    checkAuthGuard,
    applyLoginResult
  }
})
