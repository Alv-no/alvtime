<template>
  <md-snackbar
    :md-position="position"
    :md-duration="duration"
    :md-active.sync="show"
    md-persistent
  >
    <span>Det ser ut som at du har mistet tilgangen til internett.</span>
    <md-button class="icon_button" @click="close">
      <md-icon class="icon">close</md-icon>
      <Tooltip text="Lukk" />
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
      show: false,
    };
  },

  computed: {
    isOnline(): boolean {
      return !!this.$store.state.isOnline;
    },
  },

  watch: {
    isOnline() {
      this.show = !this.isOnline;
    },
  },

  created() {
    this.$store.commit("UPDATE_ONLINE_STATUS");
    window.addEventListener("online", () =>
      this.$store.commit("UPDATE_ONLINE_STATUS")
    );
    window.addEventListener("offline", () =>
      this.$store.commit("UPDATE_ONLINE_STATUS")
    );
  },

  methods: {
    close() {
      this.show = false;
    },
  },
});
</script>

<style scoped>
.md-snackbar.md-theme-default {
  color: #000;
  font-weight: 500;
  background-color: #e8b925;
}

.md-button.md-theme-default {
  color: #000;
}

.md-button.md-theme-default .md-icon-font {
  color: #000;
}

.icon_button {
  min-width: 1.5rem !important;
}

.tooltip {
  z-index: 15;
}
</style>
