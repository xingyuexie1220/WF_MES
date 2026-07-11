import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import {
  getRoutersApi,
  getUserInfoApi,
  loginApi,
  logoutApi,
  selectFactoryApi,
  switchFactoryApi
} from '@/api/auth'
import { CLIENT_WEB } from '@/types/common/enums'
import type { LoginRequest, SelectFactoryRequest } from '@/types/auth/login'
import type { RouterMenu, UserInfo } from '@/types/auth/user'
import router from '@/router'
import { useMenuStore } from '@/stores/layout/menu'
import { useTabsStore } from '@/stores/layout/tabs'
import i18n from '@/i18n'
import { getAccessToken, getRefreshToken, setFactoryId, setTokens, clearAllStorage } from '@/utils/auth/token'

export const useUserStore = defineStore('wf-user', () => {
  const token = ref(getAccessToken())
  const refreshToken = ref(getRefreshToken())
  const userInfo = ref<UserInfo | null>(null)
  const routers = ref<RouterMenu[]>([])

  const permissions = computed(() => userInfo.value?.permissions ?? [])

  async function login(form: LoginRequest) {
    const data = await loginApi({
      ...form,
      clientType: CLIENT_WEB,
      locale: i18n.global.locale.value
    })
    if (data.needSelectFactory) {
      return data
    }
    applyLoginResult(data)
    return data
  }

  async function selectFactory(form: SelectFactoryRequest) {
    const data = await selectFactoryApi({
      ...form,
      clientType: CLIENT_WEB,
      locale: i18n.global.locale.value
    })
    applyLoginResult(data)
    return data
  }

  async function switchFactory(factoryId: number) {
    const data = await switchFactoryApi({ factoryId })
    applyLoginResult(data)
    return data
  }

  function applyLoginResult(data: { accessToken: string; refreshToken: string; userInfo: UserInfo }) {
    token.value = data.accessToken
    refreshToken.value = data.refreshToken
    userInfo.value = data.userInfo
    setTokens(data.accessToken, data.refreshToken, data.userInfo.factoryId)
  }

  async function fetchUserInfo() {
    userInfo.value = await getUserInfoApi()
    if (userInfo.value?.factoryId) {
      setFactoryId(userInfo.value.factoryId)
    }
  }

  async function fetchRouters() {
    routers.value = await getRoutersApi()
  }

  async function logout() {
    try {
      await logoutApi()
    } finally {
      reset()
      clearAllStorage()
      useMenuStore().reset()
      useTabsStore().reset()
      router.push('/login')
    }
  }

  function hasPermission(code: string) {
    return userInfo.value?.roles.includes('admin') || permissions.value.includes(code)
  }

  function reset() {
    token.value = ''
    refreshToken.value = ''
    userInfo.value = null
    routers.value = []
  }

  return {
    token,
    refreshToken,
    userInfo,
    routers,
    permissions,
    login,
    selectFactory,
    switchFactory,
    fetchUserInfo,
    fetchRouters,
    logout,
    hasPermission,
    reset
  }
})
