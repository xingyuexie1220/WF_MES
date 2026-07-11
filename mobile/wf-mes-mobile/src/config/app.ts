export const CLIENT_WEB = 1
export const CLIENT_MOBILE = 2
export const CLIENT_DESKTOP = 3

export const appConfig = {
  apiBaseUrl: import.meta.env.VITE_API_BASE_URL as string,
  factoryCode: import.meta.env.VITE_FACTORY_CODE as string,
  factoryName: import.meta.env.VITE_FACTORY_NAME as string,
  appName: import.meta.env.VITE_APP_NAME as string,
  clientType: CLIENT_MOBILE
}
