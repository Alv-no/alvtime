<template>
  <md-button @click="onClick">
    <md-icon v-if="iconId">{{ iconIdToShow }}</md-icon>
    <span v-if="text" class="text">{{ text }}</span>
    <md-tooltip v-if="tooltip">{{ tooltip }}</md-tooltip>
  </md-button>
</template>

<script lang="ts">
import Vue from "vue";
export default Vue.extend({
  props: ["toggle", "iconId", "iconId2", "text", "tooltip"],

  computed: {
    iconIdToShow(): string {
      if (this.iconId && !this.iconId2) return this.iconId;
      return this.toggle ? this.iconId : this.iconId2;
    },
  },

  methods: {
    onClick() {
      this.$emit("click");
    },
  },
});
</script>

<style scoped>
.text {
  margin: 0 0.5rem;
}

.md-button.md-theme-default {
  min-width: auto;
  color: inherit;
  border: 2px solid #eabb26;
  border-radius: 30px;
}

.md-button.md-theme-default:hover {
  color: black;
  background-color: #eabb26;
  transition: background-color 500ms ease-in-out;
}

.md-toolbar.md-theme-default .md-icon {
  color: inherit;
}
</style>
