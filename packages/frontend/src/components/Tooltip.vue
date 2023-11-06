<template>
  <div class="tooltip-wrapper">
    <slot />
    <span v-if="calculatedText" class="tooltip">
      {{ calculatedText }}
    </span>
  </div>
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
.tooltip-wrapper {
  display: block;

}

.tooltip {
  display: block;
  z-index: 115;
  visibility: hidden;
  background-color: rgba(80,80,80,0.88);
  padding: 8px;
  position: relative;
}
.tooltip-wrapper:hover .tooltip {
  visibility: visible;
}
</style>
