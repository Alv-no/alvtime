<template>
  <md-snackbar
    :md-position="position"
    :md-duration="duration"
    :md-active.sync="updateExists"
    md-persistent
  >
    <span>New version available! Click to update</span>
    <md-button class="md-primary" @click="refreshApp">Reload</md-button>
  </md-snackbar>
</template>

<script>
export default {
  data() {
    const vueComponent = this;
    return {
      duration: Infinity,
      position: "left",
      refreshing: false,
      registration: null,
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
    showRefreshUI(e) {
      this.registration = e.detail;
      this.updateExists = true;
    },

    refreshApp() {
      this.updateExists = false;
      if (!this.registration || !this.registration.waiting) {
        return;
      }
      this.registration.waiting.postMessage("skipWaiting");
    },
  },
};
</script>
