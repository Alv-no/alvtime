<template>
  <md-button :disabled="disabled" @click="onClick">
    <md-icon v-if="iconId">{{ iconIdToShow }}</md-icon>
    <span v-if="text" class="text">{{ text }}</span>
    <Tooltip :text="tooltip" />
  </md-button>
</template>

<script lang="ts">
import Vue from "vue";
import Tooltip from "@/components/Tooltip.vue";

export default Vue.extend({
  components: {
    Tooltip,
  },

  props: {
    toggle: Boolean,
    iconId: {
      type: String,
      default: "",
    },
    iconId2: {
      type: String,
      default: "",
    },
    text: {
      type: String,
      default: "",
    },
    tooltip: {
      type: String,
      default: "",
    },
    disabled: {
      type: Boolean,
      default: false,
    },
  },

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
