import { createApp } from 'vue'
import { createPinia } from 'pinia'
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'
import VxeUITable from 'vxe-table'
import 'vxe-table/lib/style.css'
import VxeUIBase from 'vxe-pc-ui'
import 'vxe-pc-ui/lib/style.css'
import App from './App.vue'
import router from './router'
import i18n, { setAppLocale } from './i18n'
import { getDefaultLocale } from './i18n/config'
import './styles/index.scss'
import 'nprogress/nprogress.css'

setAppLocale(getDefaultLocale())

const app = createApp(App)

app.use(createPinia())
app.use(i18n)
app.use(router)
app.use(ElementPlus)
app.use(VxeUIBase)
app.use(VxeUITable)
app.mount('#app')
