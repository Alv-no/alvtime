<template>
  <div class="snackbar" v-if="updateExists">
    <span>New version available! Click to update</span>
    <button @click="refreshApp">
      Update
      <Tooltip text="Oppdater til siste versjon av appen" />
    </button>
  </div>
</template>

<script lang="ts">
import {defineComponent} from "vue";
import Tooltip from "@/components/Tooltip.vue";

export default defineComponent({
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
