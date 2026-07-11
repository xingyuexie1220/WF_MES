import { isLoggedIn } from '@/utils/auth'

const AUTH_FREE_PAGES = new Set([
  '/pages/login/index',
  '/pages/change-password/index'
])

function needAuth(url: string) {
  const path = url.split('?')[0]
  return !AUTH_FREE_PAGES.has(path)
}

function installRouteGuard() {
  const guard = {
    invoke(args: { url: string }) {
      if (needAuth(args.url) && !isLoggedIn()) {
        uni.reLaunch({ url: '/pages/login/index' })
        return false
      }
      return true
    }
  }

  ;(['navigateTo', 'redirectTo', 'reLaunch', 'switchTab'] as const).forEach((method) => {
    uni.addInterceptor(method, guard)
  })
}

export { installRouteGuard }
