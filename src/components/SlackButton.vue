<template>
  <md-button class="icon_button" @click="onClick">
    <md-icon md-src="/img/icons/slack.svg" />
    <md-tooltip class="tooltip">{{ tooltip }}</md-tooltip>
  </md-button>
</template>

<script lang="ts">
import Vue from "vue";
import config from "@/config.ts";

export default Vue.extend({
  props: ["tooltip"],
  methods: {
    onClick() {
      this.$emit("click");
      window.location.href = `slack://channel?team=${config.SLACK_TEAM_ID}&id=${config.SLACK_CHANNEL_ID}`;
    },
  },
});
</script>

<style scoped>
.md-icon.md-theme-default.md-icon-image svg {
  fill: #fff;
  fill: var(--md-theme-default-icon-on-background, #fff);
}

.tooltip {
  z-index: 15;
}
</style>
