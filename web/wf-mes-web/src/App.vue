<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import zhCn from 'element-plus/es/locale/lang/zh-cn'
import zhTw from 'element-plus/es/locale/lang/zh-tw'
import en from 'element-plus/es/locale/lang/en'

const { locale } = useI18n()

const elementLocale = computed(() => {
  if (locale.value === 'zh-TW') {
    return zhTw
  }
  if (locale.value === 'en') {
    return en
  }
  return zhCn
})
</script>

<template>
  <el-config-provider :locale="elementLocale">
    <router-view />
  </el-config-provider>
</template>

<style>
html,
body,
#app {
  margin: 0;
  min-height: 100%;
}
</style>
