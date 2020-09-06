<template>
  <md-snackbar
    :md-position="position"
    :md-duration="duration"
    :md-active.sync="updateExists"
    md-persistent
  >
    <span>New version available! Click to update</span>
    <md-button class="md-primary" @click="refreshApp">
      Update
      <Tooltip text="Oppdater til siste versjon av appen" />
    </md-button>
  </md-snackbar>
</template>

<script lang="ts">
import Vue from "vue";
import Tooltip from "@/components/Tooltip.vue";

export default Vue.extend({
  components: {
    Tooltip,
  },

  data() {
    return {
      duration: Infinity,
      position: "left",
      refreshing: false,
      registration: null as any,
      updateExists: false,
    };
  },

  created() {
    document.addEventListener("swUpdated", this.showRefreshUI, { once: true });
    navigator.serviceWorker.addEventListener("controllerchange", () => {
      if (this.refreshing) return;
      this.refreshing = true;
      window.location.reload();
    });
  },

  methods: {
    showRefreshUI(e: any) {
      this.registration = e.detail;
      this.updateExists = true;
    },

    refreshApp() {
      this.updateExists = false;
      if (!!this.registration && !!this.registration.waiting) {
        this.registration.waiting.postMessage("skipWaiting");
      }
    },
  },
});
</script>

<style scoped>
.tooltip {
  z-index: 15;
}
</style>
