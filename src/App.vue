<template>
  <div>
    <router-view />
    <UpdateSnackbar />
    <ErrorSnackbar />
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import moment from "moment";
import ErrorSnackbar from "@/components/ErrorSnackbar.vue";
import UpdateSnackbar from "@/components/UpdateSnackbar.vue";

export default Vue.extend({
  components: {
    ErrorSnackbar,
    UpdateSnackbar,
  },

  data() {
    return {
      pageLoadTime: moment(),
    };
  },

  computed: {
    isBecomingActive(): boolean {
      const { oldState, newState } = this.appState;
      return oldState === "passive" && newState === "active";
    },

    thirtyMinutesSinceLastPageLoad(): boolean {
      return moment().diff(this.pageLoadTime, "minutes") > 30;
    },

    appState(): { oldState: string; newState: string } {
      return this.$store.state.appState;
    },
  },

  watch: {
    appState() {
      if (
        isIPhone() &&
        this.isBecomingActive &&
        this.thirtyMinutesSinceLastPageLoad
      ) {
        location.reload();
      }
    },
  },
});

function isIPhone() {
  return /iPhone/i.test(navigator.userAgent);
}
</script>

<style>
html {
  font-size: 20px;
  font-family: "Avenir", Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  color: #2c3e50;
}
</style>
