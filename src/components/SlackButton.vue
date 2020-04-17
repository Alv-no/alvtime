<template>
  <md-button @click="onClick">
    <md-icon md-src="/img/icons/slack.svg" />
    <Tooltip :text="tooltip" />
  </md-button>
</template>

<script lang="ts">
import Vue from "vue";
import config from "@/config.ts";
import Tooltip from "@/components/Tooltip.vue";

export default Vue.extend({
  components: {
    Tooltip,
  },

  props: { tooltip: { type: String, default: "" } },
  methods: {
    onClick() {
      this.$emit("click");
      window.location.href = `slack://channel?team=${config.SLACK_TEAM_ID}&id=${config.SLACK_CHANNEL_ID}`;
    },
  },
});
</script>

<style>
path {
  fill: currentColor;
}

.tooltip {
  z-index: 15;
}
</style>
