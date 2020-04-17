<template>
  <md-tooltip v-if="calculatedText" class="tooltip">
    {{ calculatedText }}
  </md-tooltip>
</template>

<script lang="ts">
import Vue from "vue";
export default Vue.extend({
  props: {
    text: {
      type: String,
      default: "",
    },
  },

  computed: {
    calculatedText() {
      // @ts-ignore
      return this.$mq !== "sm" ? this.text : "";
    },
  },
});
</script>

<style scoped>
.tooltip {
  z-index: 15;
}
</style>
