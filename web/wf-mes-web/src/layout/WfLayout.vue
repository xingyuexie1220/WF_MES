<script setup lang="ts">
import { useAppStore } from '@/stores/layout/app'
import WfSidebar from './WfSidebar.vue'
import WfHeader from './WfHeader.vue'
import WfTagsView from './WfTagsView.vue'
import WfContent from './WfContent.vue'
import { useNoticeHub } from '@/composables/useNoticeHub'

const appStore = useAppStore()
useNoticeHub()
</script>

<template>
  <div class="wf-shell" :class="`wf-theme-${appStore.theme}`">
    <WfSidebar />

    <div class="wf-shell__main">
      <WfHeader />
      <WfTagsView />
      <WfContent />
    </div>
  </div>
</template>

<style scoped lang="scss">
.wf-shell {
  display: flex;
  width: 100%;
  height: 100vh;
  overflow: hidden;
  background: #fff;

  &__main {
    flex: 1;
    min-width: 0;
    display: flex;
    flex-direction: column;
    height: 100%;
    overflow: hidden;
  }
}
</style>
